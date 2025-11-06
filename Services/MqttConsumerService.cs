using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using backend.Data;
using backend.Hubs;
using backend.Models;

namespace backend.Services
{
    public class MqttConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MottuHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MqttConsumerService> _logger;
        private IMqttClient? _mqttClient;
        private readonly Dictionary<string, DateTime> _lastSeen = new();

        public bool IsConnected => _mqttClient?.IsConnected ?? false;

        public MqttConsumerService(
            IServiceScopeFactory scopeFactory,
            IHubContext<MottuHub> hubContext,
            IConfiguration configuration,
            ILogger<MqttConsumerService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttHost = _configuration["Mqtt:Host"] ?? "localhost";
            var mqttPort = int.Parse(_configuration["Mqtt:Port"] ?? "1883");

            _logger.LogInformation($"Starting MQTT Consumer Service - Connecting to {mqttHost}:{mqttPort}");

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttHost, mqttPort)
                .WithClientId("backend-consumer")
                .WithCleanSession()
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;

            try
            {
                await _mqttClient.ConnectAsync(options, stoppingToken);
                _logger.LogInformation("Connected to MQTT broker");

                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("mottu/uwb/+/position")
                    .Build(), stoppingToken);

                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("mottu/uwb/+/ranging")
                    .Build(), stoppingToken);

                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("mottu/motion/+")
                    .Build(), stoppingToken);

                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("mottu/status/+")
                    .Build(), stoppingToken);

                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("mottu/event/+")
                    .Build(), stoppingToken);

                _logger.LogInformation("Subscribed to MQTT topics");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MQTT Consumer Service");
            }
            finally
            {
                if (_mqttClient.IsConnected)
                {
                    await _mqttClient.DisconnectAsync();
                }
            }
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                if (topic.StartsWith("mottu/uwb/") && topic.EndsWith("/position"))
                {
                    await HandlePositionMessage(topic, payload);
                }
                else if (topic.StartsWith("mottu/uwb/") && topic.EndsWith("/ranging"))
                {
                    await HandleRangingMessage(topic, payload);
                }
                else if (topic.StartsWith("mottu/motion/"))
                {
                    await HandleMotionMessage(topic, payload);
                }
                else if (topic.StartsWith("mottu/status/"))
                {
                    await HandleStatusMessage(topic, payload);
                }
                else if (topic.StartsWith("mottu/event/"))
                {
                    await HandleEventMessage(topic, payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling MQTT message: {e.ApplicationMessage.Topic}");
            }
        }

        private async Task HandlePositionMessage(string topic, string payload)
        {
            var tagId = topic.Split('/')[2];
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            var x = data.GetProperty("x").GetDouble();
            var y = data.GetProperty("y").GetDouble();
            var ts = data.TryGetProperty("ts", out var tsElement)
                ? DateTimeOffset.FromUnixTimeSeconds((long)tsElement.GetDouble()).UtcDateTime
                : DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MottuContext>();

            var tag = await context.UwbTags.FirstOrDefaultAsync(t => t.Eui64 == tagId);
            if (tag != null)
            {
                var positionRecord = new PositionRecord
                {
                    MotoId = tag.MotoId,
                    X = x,
                    Y = y,
                    Timestamp = ts
                };

                context.PositionRecords.Add(positionRecord);

                var moto = await context.Motos.FindAsync(tag.MotoId);
                if (moto != null)
                {
                    moto.LastX = x;
                    moto.LastY = y;
                    moto.LastSeenAt = ts;
                    moto.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }

            await _hubContext.Clients.All.SendAsync("ReceivePositionUpdate", tagId, x, y, ts);
            _logger.LogDebug($"Position update: {tagId} -> ({x}, {y})");
        }

        private async Task HandleRangingMessage(string topic, string payload)
        {
            var tagId = topic.Split('/')[2];
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            var ranges = data.GetProperty("ranges");
            var ts = data.TryGetProperty("ts", out var tsElement)
                ? DateTimeOffset.FromUnixTimeSeconds((long)tsElement.GetDouble()).UtcDateTime
                : DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MottuContext>();

            var tag = await context.UwbTags.FirstOrDefaultAsync(t => t.Eui64 == tagId);
            if (tag != null)
            {
                foreach (var range in ranges.EnumerateObject())
                {
                    var anchorId = range.Name;
                    var anchor = await context.UwbAnchors.FirstOrDefaultAsync(a => a.Name == anchorId);

                    if (anchor != null)
                    {
                        double distance;
                        double? rssi = null;

                        if (range.Value.ValueKind == JsonValueKind.Object)
                        {
                            distance = range.Value.GetProperty("distance").GetDouble();
                            if (range.Value.TryGetProperty("rssi", out var rssiElement))
                            {
                                rssi = rssiElement.GetDouble();
                            }
                        }
                        else
                        {
                            distance = range.Value.GetDouble();
                        }

                        var measurement = new UwbMeasurement
                        {
                            UwbTagId = tag.Id,
                            UwbAnchorId = anchor.Id,
                            Distance = distance,
                            Rssi = rssi ?? 0,
                            Timestamp = ts
                        };

                        context.UwbMeasurements.Add(measurement);
                    }
                }

                await context.SaveChangesAsync();
            }

            await _hubContext.Clients.All.SendAsync("ReceiveRangingUpdate", tagId, ranges, ts);
        }

        private async Task HandleMotionMessage(string topic, string payload)
        {
            var tagId = topic.Split('/')[2];
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            await _hubContext.Clients.All.SendAsync("ReceiveMotionEvent", tagId, data);
            _logger.LogDebug($"Motion event: {tagId}");
        }

        private async Task HandleStatusMessage(string topic, string payload)
        {
            var tagId = topic.Split('/')[2];
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MottuContext>();

            var tag = await context.UwbTags.FirstOrDefaultAsync(t => t.Eui64 == tagId);
            if (tag != null)
            {
                tag.Status = TagStatus.Ativa;
                await context.SaveChangesAsync();
            }

            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", tagId, data);
            _logger.LogDebug($"Status update: {tagId}");
        }

        private async Task HandleEventMessage(string topic, string payload)
        {
            var tagId = topic.Split('/')[2];
            var data = JsonSerializer.Deserialize<JsonElement>(payload);

            var eventType = data.TryGetProperty("reason", out var reason)
                ? reason.GetString()
                : "unknown";

            if (eventType == "geofence_breach")
            {
                await _hubContext.Clients.All.SendAsync("ReceiveGeofenceEvent", tagId, data);
                _logger.LogWarning($"Geofence breach: {tagId}");
            }
            else if (eventType == "offline")
            {
                await _hubContext.Clients.All.SendAsync("ReceiveOfflineEvent", tagId, data);
                _logger.LogWarning($"Tag offline: {tagId}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Consumer Service");

            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}

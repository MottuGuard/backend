using Microsoft.Extensions.Diagnostics.HealthChecks;
using backend.Services;

namespace backend.HealthChecks
{
    public class MqttHealthCheck : IHealthCheck
    {
        private readonly MqttConsumerService _mqttService;
        private readonly ILogger<MqttHealthCheck> _logger;

        public MqttHealthCheck(MqttConsumerService mqttService, ILogger<MqttHealthCheck> logger)
        {
            _mqttService = mqttService;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isConnected = _mqttService.IsConnected;

                if (isConnected)
                {
                    return Task.FromResult(
                        HealthCheckResult.Healthy("MQTT broker is connected and operational"));
                }

                _logger.LogWarning("MQTT broker is not connected");
                return Task.FromResult(
                    HealthCheckResult.Degraded("MQTT broker is not connected"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for MQTT broker");
                return Task.FromResult(
                    HealthCheckResult.Unhealthy("MQTT health check failed with exception", ex));
            }
        }
    }
}

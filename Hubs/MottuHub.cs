using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
    public class MottuHub : Hub
    {
        public async Task SendPositionUpdate(string tagId, double x, double y, DateTime timestamp)
        {
            await Clients.All.SendAsync("ReceivePositionUpdate", tagId, x, y, timestamp);
        }

        public async Task SendRangingUpdate(string tagId, object ranges, DateTime timestamp)
        {
            await Clients.All.SendAsync("ReceiveRangingUpdate", tagId, ranges, timestamp);
        }

        public async Task SendStatusUpdate(string tagId, object status)
        {
            await Clients.All.SendAsync("ReceiveStatusUpdate", tagId, status);
        }

        public async Task SendMotionEvent(string tagId, object motionData)
        {
            await Clients.All.SendAsync("ReceiveMotionEvent", tagId, motionData);
        }

        public async Task SendGeofenceEvent(string tagId, object eventData)
        {
            await Clients.All.SendAsync("ReceiveGeofenceEvent", tagId, eventData);
        }

        public async Task SendOfflineEvent(string tagId, object eventData)
        {
            await Clients.All.SendAsync("ReceiveOfflineEvent", tagId, eventData);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        }
    }
}

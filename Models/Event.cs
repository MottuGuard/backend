namespace backend.Models
{
    public class Event
    {
        public int Id { get; set; }
        public UwbTag UwbTag { get; set; }
        public string Type { get; set; }
        public string? Payload { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

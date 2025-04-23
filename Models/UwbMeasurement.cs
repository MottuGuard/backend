namespace backend.Models
{
    public class UwbMeasurement
    {
        public int Id { get; set; }
        public int UwbTagId { get; set; }
        public UwbTag UwbTag { get; set; }
        public int UwbAnchorId { get; set; }
        public UwbAnchor UwbAnchor { get; set; }

        public DateTime Timestamp { get; set; }
        public double Distance { get; set; }
        public double Rssi { get; set; }
    }
}

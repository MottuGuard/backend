namespace backend.Models
{
    public class PositionRecord
    {
        public int PositionRecordId { get; set; }
        public int MotoId { get; set; }
        public Moto Moto { get; set; }

        public DateTime Timestamp { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}

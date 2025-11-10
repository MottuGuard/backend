namespace backend.DTOs.UwbMeasurement;

public class UwbMeasurementResponseDto
{
    public int Id { get; set; }

    public int UwbTagId { get; set; }

    public int UwbAnchorId { get; set; }

    public DateTime Timestamp { get; set; }

    public double Distance { get; set; }

    public double Rssi { get; set; }

    public UwbTagInfo? UwbTag { get; set; }

    public UwbAnchorInfo? UwbAnchor { get; set; }

    public class UwbTagInfo
    {
        public int Id { get; set; }
        public string Eui64 { get; set; } = string.Empty;
    }

    public class UwbAnchorInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}

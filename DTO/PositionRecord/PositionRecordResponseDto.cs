namespace backend.DTOs.PositionRecord;

public class PositionRecordResponseDto
{
    public int Id { get; set; }

    public int MotoId { get; set; }

    public DateTime Timestamp { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public MotoInfo? Moto { get; set; }

    public class MotoInfo
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
    }
}

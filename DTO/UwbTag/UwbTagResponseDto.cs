namespace backend.DTOs.UwbTag;

public class UwbTagResponseDto
{
    public int Id { get; set; }

    public string Eui64 { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int MotoId { get; set; }

    public MotoInfo? Moto { get; set; }

    public class MotoInfo
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

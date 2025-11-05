using backend.Models;

namespace backend.DTOs.Moto;

public class MotoResponseDto
{
    public int Id { get; set; }

    public string Chassi { get; set; } = string.Empty;

    public string Placa { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public double? LastX { get; set; }

    public double? LastY { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public UwbTagInfo? UwbTag { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public class UwbTagInfo
    {
        public int Id { get; set; }
        public string Eui64 { get; set; } = string.Empty;
    }
}

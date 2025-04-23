namespace backend.Models
{
    public class Moto
    {
        public int MotoId { get; set; }
        public string? Chassi { get; set; }
        public string? Placa { get; set; }
        public ModeloMoto Modelo { get; set; }

        public int UwbTagId { get; set; }
        public UwbTag UwbTag { get; set; }

        public MotoStatus Status { get; set; } = MotoStatus.Disponivel;

        public double? LastX { get; set; }
        public double? LastY { get; set; }
        public DateTime? LastSeenAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<UwbMeasurement> Measurements { get; set; }
            = new List<UwbMeasurement>();
        public List<PositionRecord> PositionHistory { get; set; }
            = new List<PositionRecord>();
    }

    public enum ModeloMoto
    {
        MottuSportESD,
        MottuSport,
        MottuE,
        MottuPop
    }
    public enum MotoStatus
    {
        Disponivel, 
        Reservada, 
        EmManutencao,
    }
}

namespace backend.Models
{
    public class Moto
    {
        public int MotoId { get; set; }
        public string? Chassi { get; set; }
        public string? Placa { get; set; }
        public ModeloMoto Modelo { get; set; }
    }

    public enum ModeloMoto
    {
        MottuSportESD,
        MottuSport,
        MottuE,
        MottuPop
    }
}

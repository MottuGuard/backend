namespace backend.Models
{
    public class UwbTag
    {
        public int Id { get; set; }
        public string Eui64 { get; set; }
        public TagStatus Status { get; set; } = TagStatus.Inativa;
        public int MotoId { get; set; }
        public Moto Moto { get; set; }
    }

    public enum TagStatus { Ativa, Inativa, Manutencao }
}

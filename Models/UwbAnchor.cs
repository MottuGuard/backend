namespace backend.Models
{
    public class UwbAnchor
    {
        public int UwbAnchorId { get; set; }
        public string Name { get; set; }
        //metros, referencia local
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}

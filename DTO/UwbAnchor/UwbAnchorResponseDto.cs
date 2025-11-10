namespace backend.DTOs.UwbAnchor;

public class UwbAnchorResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }
}

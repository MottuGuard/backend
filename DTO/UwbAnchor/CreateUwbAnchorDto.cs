using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.UwbAnchor;

public class CreateUwbAnchorDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name must not exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "X coordinate is required")]
    public double X { get; set; }

    [Required(ErrorMessage = "Y coordinate is required")]
    public double Y { get; set; }

    [Required(ErrorMessage = "Z coordinate is required")]
    public double Z { get; set; }
}

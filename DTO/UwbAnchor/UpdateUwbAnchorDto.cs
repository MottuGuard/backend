using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.UwbAnchor;

public class UpdateUwbAnchorDto
{
    [StringLength(50, ErrorMessage = "Name must not exceed 50 characters")]
    public string? Name { get; set; }

    public double? X { get; set; }

    public double? Y { get; set; }

    public double? Z { get; set; }
}

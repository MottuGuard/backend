using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.UwbTag;

public class CreateUwbTagDto
{
    [Required(ErrorMessage = "Eui64 is required")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "Eui64 must be exactly 16 characters")]
    [RegularExpression(@"^[0-9A-Fa-f]{16}$", ErrorMessage = "Eui64 must be a valid 16-character hexadecimal value")]
    public string Eui64 { get; set; } = string.Empty;

    [Required(ErrorMessage = "MotoId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MotoId must be a valid positive integer")]
    public int MotoId { get; set; }

    [EnumDataType(typeof(TagStatus), ErrorMessage = "Invalid status value")]
    public TagStatus Status { get; set; } = TagStatus.Inativa;
}

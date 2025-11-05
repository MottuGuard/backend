using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Moto;

public class CreateMotoDto
{
    [Required(ErrorMessage = "Chassi is required")]
    [StringLength(50, ErrorMessage = "Chassi must not exceed 50 characters")]
    public string Chassi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Placa is required")]
    [StringLength(20, ErrorMessage = "Placa must not exceed 20 characters")]
    [RegularExpression(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$", ErrorMessage = "Placa must follow Brazilian format (e.g., ABC1D23)")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Modelo is required")]
    [EnumDataType(typeof(ModeloMoto), ErrorMessage = "Invalid modelo value")]
    public ModeloMoto Modelo { get; set; }

    [EnumDataType(typeof(MotoStatus), ErrorMessage = "Invalid status value")]
    public MotoStatus Status { get; set; } = MotoStatus.Disponivel;
}

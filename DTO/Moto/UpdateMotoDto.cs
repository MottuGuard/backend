using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Moto;

public class UpdateMotoDto
{
    [StringLength(50, ErrorMessage = "Chassi must not exceed 50 characters")]
    public string? Chassi { get; set; }

    [StringLength(20, ErrorMessage = "Placa must not exceed 20 characters")]
    public string? Placa { get; set; }

    [EnumDataType(typeof(ModeloMoto), ErrorMessage = "Invalid modelo value")]
    public ModeloMoto? Modelo { get; set; }

    [EnumDataType(typeof(MotoStatus), ErrorMessage = "Invalid status value")]
    public MotoStatus? Status { get; set; }
}

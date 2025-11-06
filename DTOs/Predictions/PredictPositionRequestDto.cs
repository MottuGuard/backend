using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Predictions
{
    /// <summary>
    /// Requisição para predição de posição
    /// </summary>
    public class PredictPositionRequestDto
    {
        /// <summary>
        /// ID da moto para predizer próxima posição
        /// </summary>
        [Required]
        public int MotoId { get; set; }

        /// <summary>
        /// Número de posições recentes a considerar (padrão: 5, mín: 3, máx: 10)
        /// </summary>
        [Range(3, 10)]
        public int? HistoryLength { get; set; } = 5;
    }
}

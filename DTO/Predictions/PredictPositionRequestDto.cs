using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Predictions
{
    public class PredictPositionRequestDto
    {
        [Required]
        public int MotoId { get; set; }

        [Range(3, 10)]
        public int? HistoryLength { get; set; } = 5;
    }
}

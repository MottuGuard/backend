namespace backend.DTOs.Predictions
{
    public class ModelMetricsResponseDto
    {
        public bool IsModelTrained { get; set; }

        public DateTime? LastTrainedAt { get; set; }

        public int? TrainingSampleCount { get; set; }

        public double? MaeX { get; set; }

        public double? MaeY { get; set; }

        public double? RmseX { get; set; }

        public double? RmseY { get; set; }

        public double? RSquaredX { get; set; }

        public double? RSquaredY { get; set; }

        public string? QualityAssessment { get; set; }

        public string? Notes { get; set; }
    }
}

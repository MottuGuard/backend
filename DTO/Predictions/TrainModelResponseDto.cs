namespace backend.DTOs.Predictions
{
    public class TrainModelResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TrainingSamples { get; set; }

        public double? MaeX { get; set; }

        public double? MaeY { get; set; }

        public double? RSquaredX { get; set; }

        public double? RSquaredY { get; set; }

        public double TrainingTimeSeconds { get; set; }
    }
}

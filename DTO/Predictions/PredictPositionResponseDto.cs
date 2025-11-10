namespace backend.DTOs.Predictions
{
    public class PredictPositionResponseDto
    {
        public int MotoId { get; set; }

        public double CurrentX { get; set; }

        public double CurrentY { get; set; }

        public double PredictedNextX { get; set; }

        public double PredictedNextY { get; set; }

        public double PredictedDistance { get; set; }

        public double PredictedDirection { get; set; }

        public double EstimatedTimeToNext { get; set; }

        public DateTime PredictionTimestamp { get; set; }

        public int HistoricalPointsUsed { get; set; }
    }
}

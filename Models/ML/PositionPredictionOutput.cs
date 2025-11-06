using Microsoft.ML.Data;

namespace backend.Models.ML
{
    public class PositionPredictionOutput
    {
        [ColumnName("Score")]
        public float PredictedNextX { get; set; }

        public float PredictedNextY { get; set; }
    }
}

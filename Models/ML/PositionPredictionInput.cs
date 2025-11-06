using Microsoft.ML.Data;

namespace backend.Models.ML
{
    public class PositionPredictionInput
    {
        [LoadColumn(0)]
        public float CurrentX { get; set; }

        [LoadColumn(1)]
        public float CurrentY { get; set; }

        [LoadColumn(2)]
        public float PreviousX { get; set; }

        [LoadColumn(3)]
        public float PreviousY { get; set; }

        [LoadColumn(4)]
        public float Position2X { get; set; }

        [LoadColumn(5)]
        public float Position2Y { get; set; }

        [LoadColumn(6)]
        public float Position3X { get; set; }

        [LoadColumn(7)]
        public float Position3Y { get; set; }

        [LoadColumn(8)]
        public float Position4X { get; set; }

        [LoadColumn(9)]
        public float Position4Y { get; set; }

        [LoadColumn(10)]
        public float VelocityX { get; set; }

        [LoadColumn(11)]
        public float VelocityY { get; set; }

        [LoadColumn(12)]
        public float AvgTimeDelta { get; set; }

        [LoadColumn(13)]
        public float Speed { get; set; }

        [LoadColumn(14)]
        [ColumnName("NextX")]
        public float NextX { get; set; }

        [LoadColumn(15)]
        [ColumnName("NextY")]
        public float NextY { get; set; }
    }
}

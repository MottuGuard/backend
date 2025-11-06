namespace backend.DTOs.Predictions
{
    /// <summary>
    /// Resposta da predição de posição
    /// </summary>
    public class PredictPositionResponseDto
    {
        /// <summary>
        /// ID da moto
        /// </summary>
        public int MotoId { get; set; }

        /// <summary>
        /// Coordenada X atual
        /// </summary>
        public double CurrentX { get; set; }

        /// <summary>
        /// Coordenada Y atual
        /// </summary>
        public double CurrentY { get; set; }

        /// <summary>
        /// Coordenada X predita
        /// </summary>
        public double PredictedNextX { get; set; }

        /// <summary>
        /// Coordenada Y predita
        /// </summary>
        public double PredictedNextY { get; set; }

        /// <summary>
        /// Distância predita da posição atual
        /// </summary>
        public double PredictedDistance { get; set; }

        /// <summary>
        /// Direção predita em graus (0-360)
        /// </summary>
        public double PredictedDirection { get; set; }

        /// <summary>
        /// Tempo estimado para próxima predição (segundos)
        /// </summary>
        public double EstimatedTimeToNext { get; set; }

        /// <summary>
        /// Timestamp da predição
        /// </summary>
        public DateTime PredictionTimestamp { get; set; }

        /// <summary>
        /// Número de posições históricas usadas para predição
        /// </summary>
        public int HistoricalPointsUsed { get; set; }
    }
}

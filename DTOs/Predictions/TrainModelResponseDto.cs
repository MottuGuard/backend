namespace backend.DTOs.Predictions
{
    /// <summary>
    /// Resposta do endpoint de treinamento do modelo
    /// </summary>
    public class TrainModelResponseDto
    {
        /// <summary>
        /// Indica se o treinamento foi bem-sucedido
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem descrevendo o resultado do treinamento
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Número de amostras usadas no treinamento
        /// </summary>
        public int TrainingSamples { get; set; }

        /// <summary>
        /// Erro Absoluto Médio para predição da coordenada X
        /// </summary>
        public double? MaeX { get; set; }

        /// <summary>
        /// Erro Absoluto Médio para predição da coordenada Y
        /// </summary>
        public double? MaeY { get; set; }

        /// <summary>
        /// Score R² para predição da coordenada X
        /// </summary>
        public double? RSquaredX { get; set; }

        /// <summary>
        /// Score R² para predição da coordenada Y
        /// </summary>
        public double? RSquaredY { get; set; }

        /// <summary>
        /// Tempo em segundos para treinar o modelo
        /// </summary>
        public double TrainingTimeSeconds { get; set; }
    }
}

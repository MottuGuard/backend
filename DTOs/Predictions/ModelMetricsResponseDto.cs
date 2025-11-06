namespace backend.DTOs.Predictions
{
    /// <summary>
    /// Resposta com métricas e avaliação do modelo
    /// </summary>
    public class ModelMetricsResponseDto
    {
        /// <summary>
        /// Indica se o modelo foi treinado
        /// </summary>
        public bool IsModelTrained { get; set; }

        /// <summary>
        /// Data/hora do último treinamento do modelo
        /// </summary>
        public DateTime? LastTrainedAt { get; set; }

        /// <summary>
        /// Número de amostras usadas no treinamento
        /// </summary>
        public int? TrainingSampleCount { get; set; }

        /// <summary>
        /// Erro Absoluto Médio para predição da coordenada X
        /// </summary>
        public double? MaeX { get; set; }

        /// <summary>
        /// Erro Absoluto Médio para predição da coordenada Y
        /// </summary>
        public double? MaeY { get; set; }

        /// <summary>
        /// Raiz do Erro Quadrático Médio para predição da coordenada X
        /// </summary>
        public double? RmseX { get; set; }

        /// <summary>
        /// Raiz do Erro Quadrático Médio para predição da coordenada Y
        /// </summary>
        public double? RmseY { get; set; }

        /// <summary>
        /// Score R² para predição da coordenada X (0-1, quanto maior melhor)
        /// </summary>
        public double? RSquaredX { get; set; }

        /// <summary>
        /// Score R² para predição da coordenada Y (0-1, quanto maior melhor)
        /// </summary>
        public double? RSquaredY { get; set; }

        /// <summary>
        /// Avaliação geral da qualidade do modelo
        /// </summary>
        public string? QualityAssessment { get; set; }

        /// <summary>
        /// Observações ou avisos adicionais sobre o modelo
        /// </summary>
        public string? Notes { get; set; }
    }
}

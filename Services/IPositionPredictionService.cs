using backend.DTOs.Predictions;

namespace backend.Services
{
    /// <summary>
    /// Serviço para predição de posição de motos usando ML
    /// </summary>
    public interface IPositionPredictionService
    {
        /// <summary>
        /// Treina o modelo ML usando dados históricos de posição
        /// </summary>
        /// <param name="minSamplesRequired">Número mínimo de amostras necessárias para treinamento (padrão: 50)</param>
        /// <returns>Resultado do treinamento com métricas</returns>
        Task<TrainModelResponseDto> TrainModelAsync(int minSamplesRequired = 50);

        /// <summary>
        /// Prediz a próxima posição de uma moto específica
        /// </summary>
        /// <param name="motoId">ID da moto</param>
        /// <param name="historyLength">Número de posições recentes a considerar (3-10)</param>
        /// <returns>Próxima posição predita</returns>
        Task<PredictPositionResponseDto?> PredictNextPositionAsync(int motoId, int historyLength = 5);

        /// <summary>
        /// Obtém métricas atuais e estatísticas de avaliação do modelo
        /// </summary>
        /// <returns>Métricas e estatísticas de performance do modelo</returns>
        Task<ModelMetricsResponseDto> GetModelMetricsAsync();

        /// <summary>
        /// Verifica se o modelo ML está treinado e pronto para predições
        /// </summary>
        /// <returns>True se o modelo está treinado, false caso contrário</returns>
        bool IsModelTrained();
    }
}

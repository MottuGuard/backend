using backend.DTOs.Predictions;

namespace backend.Services
{
    public interface IPositionPredictionService
    {
        Task<TrainModelResponseDto> TrainModelAsync(int minSamplesRequired = 50);

        Task<PredictPositionResponseDto?> PredictNextPositionAsync(int motoId, int historyLength = 5);

        Task<ModelMetricsResponseDto> GetModelMetricsAsync();

        bool IsModelTrained();
    }
}

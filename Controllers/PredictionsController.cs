using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Predictions;
using backend.Models.ApiResponses;
using backend.Services;

namespace backend.Controllers
{
    /// <summary>
    /// Endpoints de predição de posição baseados em ML.NET para gerenciamento de frota de motos
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class PredictionsController : ControllerBase
    {
        private readonly IPositionPredictionService _predictionService;

        public PredictionsController(IPositionPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        /// <summary>
        /// Treina o modelo ML usando dados históricos de posição
        /// </summary>
        /// <param name="minSamples">Número mínimo de amostras de treinamento necessárias (padrão: 50)</param>
        /// <returns>Resultados do treinamento incluindo métricas do modelo</returns>
        /// <remarks>
        /// Exemplo de requisição:
        ///
        ///     POST /api/v1/Predictions/Train?minSamples=50
        ///
        /// Este endpoint treina um modelo de regressão ML.NET usando o algoritmo FastTree.
        /// O modelo aprende a partir de dados históricos de posição de motos para predizer posições futuras.
        ///
        /// Processo de Treinamento:
        /// - Extrai registros históricos de posição do banco de dados
        /// - Cria vetores de características a partir de sequências de posições (5 pontos de histórico)
        /// - Treina modelos separados para predição das coordenadas X e Y
        /// - Avalia a performance do modelo usando métricas R² e MAE
        /// - Salva os modelos treinados em disco para predições futuras
        ///
        /// Requisitos:
        /// - Pelo menos 'minSamples' registros de posição entre todas as motos
        /// - Motos devem ter pelo menos 6 registros sequenciais de posição
        /// </remarks>
        [HttpPost("Train")]
        [ProducesResponseType(typeof(TrainModelResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TrainModelResponseDto>> TrainModel([FromQuery] int minSamples = 50)
        {
            try
            {
                if (minSamples < 10)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "INVALID_MIN_SAMPLES",
                        Message = "Minimum samples must be at least 10",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var result = await _predictionService.TrainModelAsync(minSamples);

                if (!result.Success)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "TRAINING_FAILED",
                        Message = result.Message,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "TRAINING_ERROR",
                    Message = $"An error occurred during model training: {ex.Message}",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Prediz a próxima posição de uma moto específica
        /// </summary>
        /// <param name="request">Requisição de predição contendo MotoId</param>
        /// <returns>Próxima posição predita com métricas adicionais de movimento</returns>
        /// <remarks>
        /// Exemplo de requisição:
        ///
        ///     POST /api/v1/Predictions/PredictNext
        ///     {
        ///         "motoId": 1,
        ///         "historyLength": 5
        ///     }
        ///
        /// Este endpoint usa o modelo ML treinado para predizer onde a moto se moverá a seguir.
        ///
        /// Detalhes da Predição:
        /// - Usa os últimos 5 registros de posição da moto especificada
        /// - Calcula velocidade, aceleração e padrões de movimento
        /// - Prediz as próximas coordenadas X e Y
        /// - Retorna distância predita, direção e tempo estimado para alcançar a próxima posição
        ///
        /// Requisitos:
        /// - Modelo deve ser treinado primeiro (use o endpoint /Train)
        /// - Moto deve ter pelo menos 5 registros recentes de posição
        /// </remarks>
        [HttpPost("PredictNext")]
        [ProducesResponseType(typeof(PredictPositionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PredictPositionResponseDto>> PredictNextPosition(
            [FromBody] PredictPositionRequestDto request)
        {
            try
            {
                if (!_predictionService.IsModelTrained())
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "MODEL_NOT_TRAINED",
                        Message = "ML model has not been trained yet. Please train the model first using the /Train endpoint.",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var prediction = await _predictionService.PredictNextPositionAsync(
                    request.MotoId,
                    request.HistoryLength ?? 5);

                if (prediction == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "INSUFFICIENT_DATA",
                        Message = $"Motorcycle with ID {request.MotoId} does not have enough position history for prediction (needs at least 5 records).",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "PREDICTION_ERROR",
                    Message = $"An error occurred during prediction: {ex.Message}",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Obtém métricas atuais e estatísticas de avaliação do modelo ML
        /// </summary>
        /// <returns>Métricas de performance do modelo incluindo MAE, RMSE e scores R²</returns>
        /// <remarks>
        /// Exemplo de requisição:
        ///
        ///     GET /api/v1/Predictions/Metrics
        ///
        /// Este endpoint retorna métricas abrangentes sobre a performance do modelo ML treinado.
        ///
        /// Métricas Explicadas:
        /// - **MAE (Erro Absoluto Médio)**: Diferença absoluta média entre posições preditas e reais (quanto menor, melhor)
        /// - **RMSE (Raiz do Erro Quadrático Médio)**: Raiz quadrada da média dos erros ao quadrado (quanto menor, melhor)
        /// - **R² (R-Quadrado)**: Coeficiente de determinação, indica quão bem as predições se ajustam aos dados reais (0-1, quanto maior, melhor)
        ///   - R² > 0.9: Modelo excelente
        ///   - R² > 0.7: Modelo bom
        ///   - R² > 0.5: Modelo razoável
        ///   - R² &lt; 0.5: Modelo fraco
        ///
        /// O endpoint fornece métricas separadas para predições das coordenadas X e Y.
        /// </remarks>
        [HttpGet("Metrics")]
        [ProducesResponseType(typeof(ModelMetricsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ModelMetricsResponseDto>> GetMetrics()
        {
            try
            {
                var metrics = await _predictionService.GetModelMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Error = "METRICS_ERROR",
                    Message = $"An error occurred while retrieving metrics: {ex.Message}",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}

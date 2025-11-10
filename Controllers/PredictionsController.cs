using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Predictions;
using backend.Models.ApiResponses;
using backend.Services;

namespace backend.Controllers
{
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

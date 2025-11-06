using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Predictions;
using backend.Models.ML;
using System.Diagnostics;

namespace backend.Services
{
    public class PositionPredictionService : IPositionPredictionService
    {
        private readonly MottuContext _context;
        private readonly MLContext _mlContext;
        private readonly string _modelPathX;
        private readonly string _modelPathY;
        private ITransformer? _modelX;
        private ITransformer? _modelY;
        private DateTime? _lastTrainedAt;
        private int? _trainingSampleCount;
        private RegressionMetrics? _metricsX;
        private RegressionMetrics? _metricsY;
        private readonly object _lock = new();

        public PositionPredictionService(MottuContext context)
        {
            _context = context;
            _mlContext = new MLContext(seed: 42);

            var modelsDir = Path.Combine(Directory.GetCurrentDirectory(), "MLModels");
            if (!Directory.Exists(modelsDir))
            {
                Directory.CreateDirectory(modelsDir);
            }

            _modelPathX = Path.Combine(modelsDir, "position_prediction_x.zip");
            _modelPathY = Path.Combine(modelsDir, "position_prediction_y.zip");

            LoadModels();
        }

        private void LoadModels()
        {
            try
            {
                if (File.Exists(_modelPathX))
                {
                    _modelX = _mlContext.Model.Load(_modelPathX, out _);
                }
                if (File.Exists(_modelPathY))
                {
                    _modelY = _mlContext.Model.Load(_modelPathY, out _);
                }
            }
            catch (Exception)
            {
                _modelX = null;
                _modelY = null;
            }
        }

        public bool IsModelTrained()
        {
            return _modelX != null && _modelY != null;
        }

        public async Task<TrainModelResponseDto> TrainModelAsync(int minSamplesRequired = 50)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var trainingData = await ExtractTrainingDataAsync();

                if (trainingData.Count < minSamplesRequired)
                {
                    return new TrainModelResponseDto
                    {
                        Success = false,
                        Message = $"Insufficient training data. Found {trainingData.Count} samples, need at least {minSamplesRequired}.",
                        TrainingSamples = trainingData.Count
                    };
                }

                lock (_lock)
                {
                    var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                    var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

                    var pipelineX = BuildPipeline("NextX");
                    _modelX = pipelineX.Fit(split.TrainSet);

                    var pipelineY = BuildPipeline("NextY");
                    _modelY = pipelineY.Fit(split.TrainSet);

                    var predictionsX = _modelX.Transform(split.TestSet);
                    _metricsX = _mlContext.Regression.Evaluate(predictionsX, labelColumnName: "NextX");

                    var predictionsY = _modelY.Transform(split.TestSet);
                    _metricsY = _mlContext.Regression.Evaluate(predictionsY, labelColumnName: "NextY");

                    _mlContext.Model.Save(_modelX, dataView.Schema, _modelPathX);
                    _mlContext.Model.Save(_modelY, dataView.Schema, _modelPathY);

                    _lastTrainedAt = DateTime.UtcNow;
                    _trainingSampleCount = trainingData.Count;
                }

                stopwatch.Stop();

                return new TrainModelResponseDto
                {
                    Success = true,
                    Message = "Model trained successfully",
                    TrainingSamples = trainingData.Count,
                    MaeX = _metricsX?.MeanAbsoluteError,
                    MaeY = _metricsY?.MeanAbsoluteError,
                    RSquaredX = _metricsX?.RSquared,
                    RSquaredY = _metricsY?.RSquared,
                    TrainingTimeSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new TrainModelResponseDto
                {
                    Success = false,
                    Message = $"Training failed: {ex.Message}",
                    TrainingSamples = 0
                };
            }
        }

        private IEstimator<ITransformer> BuildPipeline(string labelColumn)
        {
            var featureColumns = new[]
            {
                "CurrentX", "CurrentY",
                "PreviousX", "PreviousY",
                "Position2X", "Position2Y",
                "Position3X", "Position3Y",
                "Position4X", "Position4Y",
                "VelocityX", "VelocityY",
                "AvgTimeDelta", "Speed"
            };

            return _mlContext.Transforms.Concatenate("Features", featureColumns)
                .Append(_mlContext.Regression.Trainers.FastTree(
                    labelColumnName: labelColumn,
                    featureColumnName: "Features",
                    numberOfLeaves: 20,
                    minimumExampleCountPerLeaf: 10,
                    numberOfTrees: 100));
        }

        private async Task<List<PositionPredictionInput>> ExtractTrainingDataAsync()
        {
            var trainingData = new List<PositionPredictionInput>();

            var motos = await _context.Motos
                .Include(m => m.PositionHistory)
                .Where(m => m.PositionHistory.Count >= 6)
                .ToListAsync();

            foreach (var moto in motos)
            {
                var positions = moto.PositionHistory
                    .OrderBy(p => p.Timestamp)
                    .ToList();

                for (int i = 0; i <= positions.Count - 6; i++)
                {
                    var window = positions.Skip(i).Take(6).ToList();
                    var sample = CreateTrainingSample(window);
                    if (sample != null)
                    {
                        trainingData.Add(sample);
                    }
                }
            }

            return trainingData;
        }

        private PositionPredictionInput? CreateTrainingSample(List<Models.PositionRecord> window)
        {
            if (window.Count != 6) return null;

            var timeDelta1 = (window[1].Timestamp - window[0].Timestamp).TotalSeconds;
            var timeDelta2 = (window[2].Timestamp - window[1].Timestamp).TotalSeconds;
            var timeDelta3 = (window[3].Timestamp - window[2].Timestamp).TotalSeconds;
            var timeDelta4 = (window[4].Timestamp - window[3].Timestamp).TotalSeconds;

            if (timeDelta1 <= 0 || timeDelta2 <= 0 || timeDelta3 <= 0 || timeDelta4 <= 0)
                return null;

            var avgTimeDelta = (timeDelta1 + timeDelta2 + timeDelta3 + timeDelta4) / 4.0;

            var velocityX = (window[4].X - window[0].X) / (avgTimeDelta * 4);
            var velocityY = (window[4].Y - window[0].Y) / (avgTimeDelta * 4);
            var speed = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);

            return new PositionPredictionInput
            {
                CurrentX = (float)window[4].X,
                CurrentY = (float)window[4].Y,
                PreviousX = (float)window[3].X,
                PreviousY = (float)window[3].Y,
                Position2X = (float)window[2].X,
                Position2Y = (float)window[2].Y,
                Position3X = (float)window[1].X,
                Position3Y = (float)window[1].Y,
                Position4X = (float)window[0].X,
                Position4Y = (float)window[0].Y,
                VelocityX = (float)velocityX,
                VelocityY = (float)velocityY,
                AvgTimeDelta = (float)avgTimeDelta,
                Speed = (float)speed,
                NextX = (float)window[5].X,
                NextY = (float)window[5].Y
            };
        }

        public async Task<PredictPositionResponseDto?> PredictNextPositionAsync(int motoId, int historyLength = 5)
        {
            if (!IsModelTrained())
            {
                return null;
            }

            var positions = await _context.PositionRecords
                .Where(p => p.MotoId == motoId)
                .OrderByDescending(p => p.Timestamp)
                .Take(5)
                .OrderBy(p => p.Timestamp)
                .ToListAsync();

            if (positions.Count < 5)
            {
                return null;
            }

            var input = CreatePredictionInput(positions);
            if (input == null)
            {
                return null;
            }

            lock (_lock)
            {
                var predEngineX = _mlContext.Model.CreatePredictionEngine<PositionPredictionInput, PositionPredictionOutput>(_modelX!);
                var predEngineY = _mlContext.Model.CreatePredictionEngine<PositionPredictionInput, PositionPredictionOutput>(_modelY!);

                var predictionX = predEngineX.Predict(input);
                var predictionY = predEngineY.Predict(input);

                var currentPos = positions.Last();
                var predictedX = predictionX.PredictedNextX;
                var predictedY = predictionY.PredictedNextX;

                var distance = Math.Sqrt(
                    Math.Pow(predictedX - currentPos.X, 2) +
                    Math.Pow(predictedY - currentPos.Y, 2)
                );

                var direction = Math.Atan2(predictedY - currentPos.Y, predictedX - currentPos.X) * (180 / Math.PI);
                if (direction < 0) direction += 360;

                return new PredictPositionResponseDto
                {
                    MotoId = motoId,
                    CurrentX = currentPos.X,
                    CurrentY = currentPos.Y,
                    PredictedNextX = predictedX,
                    PredictedNextY = predictedY,
                    PredictedDistance = distance,
                    PredictedDirection = direction,
                    EstimatedTimeToNext = input.AvgTimeDelta,
                    PredictionTimestamp = DateTime.UtcNow,
                    HistoricalPointsUsed = positions.Count
                };
            }
        }

        private PositionPredictionInput? CreatePredictionInput(List<Models.PositionRecord> positions)
        {
            if (positions.Count < 5) return null;

            var timeDelta1 = (positions[1].Timestamp - positions[0].Timestamp).TotalSeconds;
            var timeDelta2 = (positions[2].Timestamp - positions[1].Timestamp).TotalSeconds;
            var timeDelta3 = (positions[3].Timestamp - positions[2].Timestamp).TotalSeconds;
            var timeDelta4 = (positions[4].Timestamp - positions[3].Timestamp).TotalSeconds;

            if (timeDelta1 <= 0 || timeDelta2 <= 0 || timeDelta3 <= 0 || timeDelta4 <= 0)
                return null;

            var avgTimeDelta = (timeDelta1 + timeDelta2 + timeDelta3 + timeDelta4) / 4.0;

            var velocityX = (positions[4].X - positions[0].X) / (avgTimeDelta * 4);
            var velocityY = (positions[4].Y - positions[0].Y) / (avgTimeDelta * 4);
            var speed = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);

            return new PositionPredictionInput
            {
                CurrentX = (float)positions[4].X,
                CurrentY = (float)positions[4].Y,
                PreviousX = (float)positions[3].X,
                PreviousY = (float)positions[3].Y,
                Position2X = (float)positions[2].X,
                Position2Y = (float)positions[2].Y,
                Position3X = (float)positions[1].X,
                Position3Y = (float)positions[1].Y,
                Position4X = (float)positions[0].X,
                Position4Y = (float)positions[0].Y,
                VelocityX = (float)velocityX,
                VelocityY = (float)velocityY,
                AvgTimeDelta = (float)avgTimeDelta,
                Speed = (float)speed,
                NextX = 0,
                NextY = 0
            };
        }

        public async Task<ModelMetricsResponseDto> GetModelMetricsAsync()
        {
            var response = new ModelMetricsResponseDto
            {
                IsModelTrained = IsModelTrained(),
                LastTrainedAt = _lastTrainedAt,
                TrainingSampleCount = _trainingSampleCount
            };

            if (_metricsX != null && _metricsY != null)
            {
                response.MaeX = _metricsX.MeanAbsoluteError;
                response.MaeY = _metricsY.MeanAbsoluteError;
                response.RmseX = _metricsX.RootMeanSquaredError;
                response.RmseY = _metricsY.RootMeanSquaredError;
                response.RSquaredX = _metricsX.RSquared;
                response.RSquaredY = _metricsY.RSquared;

                var avgRSquared = (_metricsX.RSquared + _metricsY.RSquared) / 2.0;
                response.QualityAssessment = avgRSquared switch
                {
                    > 0.9 => "Excellent",
                    > 0.7 => "Good",
                    > 0.5 => "Fair",
                    _ => "Poor"
                };

                response.Notes = $"Model uses FastTree regression with {_trainingSampleCount} training samples. " +
                                $"Average R² score: {avgRSquared:F3}";
            }
            else
            {
                response.Notes = "Model not yet trained. Use the /Train endpoint to train the model.";
            }

            return await Task.FromResult(response);
        }
    }
}

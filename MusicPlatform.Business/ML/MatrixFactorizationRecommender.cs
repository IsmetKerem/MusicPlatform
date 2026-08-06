using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace MusicPlatform.Business.ML;

public class ListeningRecord
{
    public float UserId { get; set; }
    public float SongId { get; set; }
    public float Label { get; set; }   
}

public class ListeningPrediction
{
    public float Score { get; set; }
}


public class MatrixFactorizationRecommender
{
    public const int MinTrainingRows = 200;

    private readonly MLContext _mlContext = new(seed: 42);
    private ITransformer? _model;
    private PredictionEngine<ListeningRecord, ListeningPrediction>? _engine;
    private readonly object _lock = new();

    public bool IsTrained => _model is not null;
    public DateTime? LastTrainedAt { get; private set; }

    public bool Train(IEnumerable<ListeningRecord> data)
    {
        var rows = data.ToList();
        if (rows.Count < MinTrainingRows) return false;

        var dataView = _mlContext.Data.LoadFromEnumerable(rows);

        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = "UserIdEncoded",
            MatrixRowIndexColumnName    = "SongIdEncoded",
            LabelColumnName             = nameof(ListeningRecord.Label),
            NumberOfIterations          = 30,
            ApproximationRank           = 16,
            LearningRate                = 0.1,
            Quiet                       = true
        };

        var pipeline = _mlContext.Transforms.Conversion
            .MapValueToKey("UserIdEncoded", nameof(ListeningRecord.UserId))
            .Append(_mlContext.Transforms.Conversion
                .MapValueToKey("SongIdEncoded", nameof(ListeningRecord.SongId)))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

        lock (_lock)
        {
            _model = pipeline.Fit(dataView);
            _engine = _mlContext.Model
                .CreatePredictionEngine<ListeningRecord, ListeningPrediction>(_model);
            LastTrainedAt = DateTime.UtcNow;
        }

        return true;
    }

    public float Predict(int userId, int songId)
    {
        if (_engine is null) return 0f;

        lock (_lock)
        {
            var p = _engine.Predict(new ListeningRecord { UserId = userId, SongId = songId });
            return float.IsNaN(p.Score) ? 0f : p.Score;
        }
    }
}
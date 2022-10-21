using Microsoft.ML.Data;
using Microsoft.Extensions.ML;

var builder = WebApplication.CreateBuilder(args);

// Add PredictionEngine Pool service
builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
    .FromUri(uri: "https://github.com/dotnet/samples/raw/main/machine-learning/models/sentimentanalysis/sentiment_model.zip");

var app = builder.Build();

var predictionHandler = 
    async (PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool, ModelInput input) =>
        await Task.FromResult(predictionEnginePool.Predict(input));

app.MapPost("/predict", predictionHandler);

app.Run();

public class ModelInput
{
    public string SentimentText;

    [ColumnName("Label")]
    public bool Sentiment;
}

public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Probability { get; set; }

    public float Score { get; set; }
}
# Minimal API Sample

## Prerequisites

- [.NET 6 SDK or later](https://dotnet.microsoft.com/download)
- Your preferred IDE / code editor ([Visual Studio 2022](https://visualstudio.microsoft.com/downloads/), [Visual Studio Code](https://code.visualstudio.com/))

## Create Minimal API

In the terminal enter:

```dotnetcli
dotnet new webapi -minimal -o MLAPI
```

## Install ML.NET Packages

In the terminal enter:

### Install AutoML (this contains all ML.NET packages and dependencies)

```dotnetcli
dotnet add package Microsoft.ML.AutoML --version 0.20.0-preview.22514.1 --source https://pkgs.dev.azure.com/dnceng/public/_packaging/MachineLearning/nuget/v3/index.json
```

### Install ML extensions package

```dotnetcli
dotnet add package Microsoft.Extensions.ML --version 2.0.0-preview.22514.1 --source https://pkgs.dev.azure.com/dnceng/public/_packaging/MachineLearning/nuget/v3/index.json
```

Source is only needed in this case because it's using the daily builds. When using the NuGet feed it can be ommitted.

## Add using statements

ADd to the top of *Program.cs*:

```csharp
using Microsoft.ML.Data;
using Microsoft.Extensions.ML;
```

## Define schema classes

Create the following classes at the bottom of the *Program.cs* class.

### Model input

```csharp
public class ModelInput
{
    public string SentimentText;

    [ColumnName("Label")]
    public bool Sentiment;
}
```

### Model output

```csharp
public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Probability { get; set; }

    public float Score { get; set; }
}
```

## Register PredictionEnginePool

In this sample, it's getting the model from a public endpoint on GitHub. You can also use a file. For more info, see [Register PredictionEnginePool](https://learn.microsoft.com/dotnet/machine-learning/how-to-guides/serve-model-web-api-ml-net#register-predictionenginepool-for-use-in-the-application). 

### Using a public endpoint

```csharp
builder.Services.AddPredictionEnginePool<SentimentData, SentimentPrediction>()
    .FromUri(uri: "https://github.com/dotnet/samples/raw/main/machine-learning/models/sentimentanalysis/sentiment_model.zip");
```

### Using a file

Add file to your *.csproj* file.

```xml
<ItemGroup Label="MyModel">
    <None Include="sentiment_model.zip">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

Add to *Program.cs*:

```csharp
builder.Services.AddPredictionEnginePool<SentimentData, SentimentPrediction>()
    .FromFile(filePath: "sentiment_model.zip");
```

## Create PredictHandler

Replace the `weatherforecast` endpoint

```csharp
app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");
```

with

```csharp
var predictionHandler = 
    async (PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool, ModelInput input) =>
        await Task.FromResult(predictionEnginePool.Predict(input));

app.MapPost("/predict", predictionHandler);
```

## Run the application

Your final *Program.cs* class should look something like this:

```csharp
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
```

Removed HTTPS and OpenAPI components for simplicity. When using in production. It's recommended you use them.

In the terminal enter:

```dotnetcli
dotnet run
```

This starts a local server listening on a port (typically 5000 for http).

## Test your application

In the terminal enter:

### Bash / sh

Remember to set the port to the one your application is listening on.

```bash
curl -POST -H "Content-type: application/json" -d '{"SentimentText":"This was a very bad steak"}' 'http://localhost:<PORT>/predict'
```

### Powershell

Remember to set the port to the one your application is listening on.

```powershell
Invoke-RestMethod "http://localhost:<PORT>/predict" -Method Post -Body (@{SentimentText="This was a very bad steak"} | ConvertTo-Json) -ContentType "application/json"
```
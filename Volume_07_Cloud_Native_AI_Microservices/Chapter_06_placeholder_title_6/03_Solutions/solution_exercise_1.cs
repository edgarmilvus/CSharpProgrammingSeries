
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
#
# MIT License
# Copyright (c) 2026 Edgar Milvus
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
*/

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

// File: Program.cs
using Microsoft.ML;
using System.Text.Json;
using System.Text.Json.Serialization;

// Define input and output schemas for ML.NET
public class SentimentData
{
    [LoadColumn(0)]
    public string Text { get; set; }
}

public class SentimentPrediction : SentimentData
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}

// Minimal API Setup
var builder = WebApplication.CreateBuilder(args);

// 1. Logging Configuration: Use JSON console logger
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.TimestampFormat = "HH:mm:ss ";
    options.IncludeScopes = true;
    options.JsonWriterOptions = new JsonWriterOptions { Indented = false };
});

// 2. Load ML Model
var mlContext = new MLContext();
ITransformer model;
try
{
    // Ensure 'sentiment_model.zip' is in the output directory
    using (var stream = File.OpenRead("sentiment_model.zip"))
    {
        model = mlContext.Model.Load(stream, out _);
    }
}
catch (FileNotFoundException)
{
    // Fallback for development if model is missing (mock prediction)
    Console.WriteLine("Warning: Model file not found. Running in mock mode.");
    model = null;
}

var app = builder.Build();

// Health Check Endpoint (Required for K8s Probes)
app.MapGet("/health", () => Results.Ok("Healthy"));

// Analysis Endpoint
app.MapPost("/analyze", async (AnalysisRequest request) =>
{
    if (model == null) return Results.Ok(new { Sentiment = "Mock: Positive", Confidence = 1.0 });

    var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
    
    // Simulate async work
    await Task.Delay(10); 
    
    var prediction = predictionEngine.Predict(new SentimentData { Text = request.Text });
    
    var sentiment = prediction.Prediction ? "Positive" : "Negative";
    
    return Results.Ok(new { Sentiment = sentiment, Confidence = prediction.Probability });
});

app.Run();

public record AnalysisRequest(string Text);

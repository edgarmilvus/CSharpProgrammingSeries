
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

// Program.cs (Agent)
using Grpc.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SentimentAgent;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for HTTP/2 (required for gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// Add gRPC services
builder.Services.AddGrpc();

// Configure Structured Logging (JSON Console)
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.TimestampFormat = "HH:mm:ss ";
    options.IncludeScopes = true;
});

var app = builder.Build();

app.MapGrpcService<InferenceServiceImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();

// gRPC Service Implementation
public class InferenceServiceImpl : InferenceService.InferenceServiceBase
{
    private readonly ILogger<InferenceServiceImpl> _logger;

    public InferenceServiceImpl(ILogger<InferenceServiceImpl> logger)
    {
        _logger = logger;
    }

    public override Task<AnalysisResponse> Analyze(AnalysisRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received text to analyze: {Text}", request.Text);

        Sentiment sentiment;
        double confidence;

        if (request.Text.Contains("good", StringComparison.OrdinalIgnoreCase))
        {
            sentiment = Sentiment.Positive;
            confidence = 0.95;
        }
        else if (request.Text.Contains("bad", StringComparison.OrdinalIgnoreCase))
        {
            sentiment = Sentiment.Negative;
            confidence = 0.92;
        }
        else
        {
            sentiment = Sentiment.Neutral;
            confidence = 0.5;
        }

        _logger.LogInformation("Analysis complete. Sentiment: {Sentiment}", sentiment);

        return Task.FromResult(new AnalysisResponse
        {
            Sentiment = sentiment,
            Confidence = confidence
        });
    }
}

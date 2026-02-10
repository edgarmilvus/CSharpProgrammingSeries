
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

// Program.cs
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 8080
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

var app = builder.Build();

// Define Data Models
record AnalysisRequest(string Text);
enum Sentiment { Positive, Negative, Neutral }
record AnalysisResult(Sentiment Sentiment, double Confidence);

// Mock Inference Logic
AnalysisResult AnalyzeSentiment(string text)
{
    if (text.Contains("good", StringComparison.OrdinalIgnoreCase))
        return new AnalysisResult(Sentiment.Positive, 0.95);
    
    if (text.Contains("bad", StringComparison.OrdinalIgnoreCase))
        return new AnalysisResult(Sentiment.Negative, 0.92);

    return new AnalysisResult(Sentiment.Neutral, 0.5);
}

// Minimal API Endpoint
app.MapPost("/analyze", async (HttpContext context) =>
{
    try
    {
        var request = await context.Request.ReadFromJsonAsync<AnalysisRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return Results.BadRequest("Text is required.");
        }

        var result = AnalyzeSentiment(request.Text);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

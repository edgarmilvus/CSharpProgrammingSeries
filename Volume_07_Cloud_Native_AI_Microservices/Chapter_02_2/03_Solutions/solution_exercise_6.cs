
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_6.cs
# Description: Solution for Exercise 6
# ==========================================

// Project: SynthesisAgent.csproj (Modified from Ex 2)
// Add NuGet Packages:
// OpenTelemetry
// OpenTelemetry.Extensions.Hosting
// OpenTelemetry.Instrumentation.AspNetCore
// OpenTelemetry.Instrumentation.GrpcNetClient

// Program.cs (SynthesisAgent)
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ResearchAssistant;

var builder = WebApplication.CreateBuilder(args);

// 1. OpenTelemetry Configuration
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SynthesisAgent"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddGrpcClientInstrumentation() // Instrument outgoing gRPC calls
        .AddConsoleExporter()); // Export to stdout

// ... gRPC client setup ...

var app = builder.Build();

// 2. Custom Span & Baggage Logic
app.MapGrpcService<SynthesisServiceImpl>();
app.Run();

// Services/SynthesisServiceImpl.cs
using Grpc.Core;
using OpenTelemetry;
using OpenTelemetry.Trace;
using ResearchAssistant;

public class SynthesisServiceImpl : SynthesisService.SynthesisServiceBase
{
    private readonly SearchService.SearchServiceClient _searchClient;
    private readonly ILogger<SynthesisServiceImpl> _logger;

    public SynthesisServiceImpl(SearchService.SearchServiceClient searchClient, ILogger<SynthesisServiceImpl> logger)
    {
        _searchClient = searchClient;
        _logger = logger;
    }

    public override async Task<SynthesisResponse> Summarize(SynthesisRequest request, ServerCallContext context)
    {
        // 4. Log Correlation: Include TraceId in logs
        var currentTraceId = Tracer.CurrentSpan?.Context.TraceId;
        _logger.LogInformation("Starting Summarize. TraceId: {TraceId}", currentTraceId);

        // 3. Custom Span
        using var activity = Tracer.StartSpan("LLM_Processing");
        
        // 5. Baggage: Check for Feature Flag Header
        // In gRPC, headers are in context.RequestHeaders
        var featureFlag = context.RequestHeaders.FirstOrDefault(h => h.Key == "X-Feature-Flag")?.Value;
        
        if (featureFlag != null)
        {
            // Add to Baggage (propagates automatically to downstream calls)
            Baggage.SetBaggage("feature", featureFlag);
            _logger.LogInformation("Feature flag detected: {Flag}", featureFlag);
        }

        try
        {
            // Context Propagation: The TraceId is automatically injected into the gRPC metadata
            // by the OpenTelemetry.Instrumentation.GrpcNetClient package.
            var searchResult = await _searchClient.FindDataAsync(new SearchRequest { Query = "User Request" });

            // Simulate processing
            activity.SetTag("processed_sources", searchResult.Sources.Count);
            
            var summary = $"Summary of: {searchResult.RawData}";
            return new SynthesisResponse { Summary = summary };
        }
        catch (Exception ex)
        {
            activity.RecordException(ex);
            throw;
        }
    }
}

// Services/SearchServiceImpl.cs (Modified)
using Grpc.Core;
using OpenTelemetry;
using ResearchAssistant;

public class SearchServiceImpl : SearchService.SearchServiceBase
{
    public override Task<SearchResult> FindData(SearchRequest request, ServerCallContext context)
    {
        // 5. Baggage: Read from Context
        // OpenTelemetry Baggage is stored in the current Activity/Context
        var baggage = Baggage.GetBaggage();
        var feature = baggage.FirstOrDefault(x => x.Key == "feature").Value;

        var response = new SearchResult();
        
        // Adjust behavior based on Baggage
        if (feature == "verbose")
        {
            response.RawData = "Verbose data retrieved...";
            for (int i = 0; i < 10; i++) response.Sources.Add($"Source {i}");
        }
        else
        {
            response.RawData = "Standard data retrieved...";
            for (int i = 0; i < 5; i++) response.Sources.Add($"Source {i}");
        }

        return Task.FromResult(response);
    }
}

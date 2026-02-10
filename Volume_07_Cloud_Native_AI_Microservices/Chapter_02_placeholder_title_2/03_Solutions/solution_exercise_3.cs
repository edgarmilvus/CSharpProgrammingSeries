
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

// Program.cs (SynthesisAgent)
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Polly;
using Polly.CircuitBreaker;
using ResearchAssistant;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(50052, o => o.Protocols = HttpProtocolVersion.Http2);
});

builder.Services.AddGrpcClient<SearchService.SearchServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:50051");
})
// Interactive Challenge: Circuit Breaker Pattern
.AddPolicyHandler(Policy<HttpResponseMessage>
    .Handle<RpcException>() // Handle gRPC exceptions
    .OrTransientHttpStatusCode()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 3, 
        durationOfBreak: TimeSpan.FromSeconds(30)
    ));

builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<SynthesisServiceImpl>();
app.Run();

// Services/SynthesisServiceImpl.cs
using Grpc.Core;
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
        try
        {
            // Call the Search Agent via gRPC
            var searchResult = await _searchClient.FindDataAsync(new SearchRequest { Query = "User Request" });
            
            // Process result
            var summary = $"Summary of: {searchResult.RawData} (using {searchResult.Sources.Count} sources)";
            return new SynthesisResponse { Summary = summary };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            // Circuit breaker likely tripped or service down
            _logger.LogWarning("Search service unavailable. Returning fallback.");
            return new SynthesisResponse { Summary = "Service temporarily unavailable. Please try again later." };
        }
    }
}


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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Grpc.Net.Client;
using Polly;
using Polly.Retry;
using Grpc.Core;

public class OrchestratorService
{
    private readonly AsyncRetryPolicy<InferenceResponse> _retryPolicy;

    public OrchestratorService()
    {
        // 3. Polly Retry Policy
        // Handles transient failures (e.g., 503 Unavailable from Istio circuit breaker or network flakiness)
        _retryPolicy = Policy<InferenceResponse>
            .Handle<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
            .WaitAndRetryAsync(
                retryCount: 3, 
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Exception.Message}");
                });
    }

    public async Task<string> RouteRequest(string text)
    {
        var channel = GrpcChannel.ForAddress("http://worker-service"); // Service name in K8s
        var client = new InferenceService.InferenceServiceClient(channel);

        // Execute with resilience
        var response = await _retryPolicy.ExecuteAsync(async () => 
        {
            return await client.ProcessAsync(new InferenceRequest { Text = text });
        });

        return response.Result;
    }
}

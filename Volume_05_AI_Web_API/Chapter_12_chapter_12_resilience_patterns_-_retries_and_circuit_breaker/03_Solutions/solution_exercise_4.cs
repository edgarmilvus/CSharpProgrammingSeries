
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;
using System.Net;

namespace AiChatApi
{
    // 1. Custom Exception for Rate Limiting
    public class RateLimitException : Exception
    {
        public RateLimitException(string message) : base(message) { }
    }

    // 2. Typed Client
    public class AiServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _resiliencePolicy;

        public AiServiceClient(HttpClient httpClient, IAsyncPolicy<HttpResponseMessage> resiliencePolicy)
        {
            _httpClient = httpClient;
            _resiliencePolicy = resiliencePolicy;
        }

        public async Task<string> GetChatResponseAsync(string message)
        {
            // Execute the composite policy
            var response = await _resiliencePolicy.ExecuteAsync(async () =>
            {
                // Simulate calling the downstream AI
                var httpResponse = await _httpClient.GetAsync("/ai/generate"); // Mock endpoint
                
                if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // Requirement 4: Specific handling for 429
                    throw new RateLimitException("Rate limit exceeded");
                }

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request failed: {httpResponse.StatusCode}");
                }

                return httpResponse;
            });

            return await response.Content.ReadAsStringAsync();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AiServiceClient _aiClient;

        public ChatController(AiServiceClient aiClient)
        {
            _aiClient = aiClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            try
            {
                var response = await _aiClient.GetChatResponseAsync(request.Message);
                return Ok(new { response });
            }
            catch (BrokenCircuitException)
            {
                // Requirement 6: Handle Open Circuit (Critical Request Scenario)
                // If critical, we might use ExecuteAndCapture to attempt a bypass, 
                // but here we return 503 Service Unavailable.
                return StatusCode(503, "AI Service temporarily unavailable due to high error rate.");
            }
            catch (RateLimitException ex)
            {
                return StatusCode(429, ex.Message);
            }
        }
    }

    public class ChatRequest { public string Message { get; set; } = string.Empty; }

    // 3. Program.cs Configuration
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Requirement 2: Policy Registry & Composite Policy
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            // Specific handling for 429 (Requirement 4)
            var rateLimitPolicy = Policy<HttpResponseMessage>
                .Handle<RateLimitException>()
                .WaitAndRetryAsync(
                    retryCount: 3, 
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(2), // Fixed backoff
                    onRetry: (outcome, delay, retryCount, context) => 
                        Console.WriteLine($"Rate limit hit. Retrying in {delay.TotalSeconds}s..."));

            var circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<RateLimitException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5, 
                    durationOfBreak: TimeSpan.FromMinutes(1));

            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

            // Composite Policy (PolicyWrap)
            var resilienceStrategy = Policy.WrapAsync(
                timeoutPolicy, 
                rateLimitPolicy, 
                retryPolicy, 
                circuitBreakerPolicy
            );

            // Requirement 1 & 2: Register Typed Client with Policy
            builder.Services.AddHttpClient<AiServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://httpstat.us"); // Mock base
            })
            .AddPolicyHandler(resilienceStrategy); // Applies the policy to every request

            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
    }
}

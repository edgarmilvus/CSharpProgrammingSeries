
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

using Dapr.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace DaprAgentService
{
    public class ChatRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<AgentController> _logger;
        private readonly string _stateStoreName;

        public AgentController(DaprClient daprClient, IConfiguration configuration, ILogger<AgentController> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
            // Externalized configuration
            _stateStoreName = configuration.GetValue<string>("DaprStateStoreName") ?? "statestore";
        }

        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody] ChatRequest request)
        {
            // Composite key for state management
            var stateKey = $"{request.UserId}_{request.ConversationId}";

            try
            {
                // 1. Retrieve context from Dapr State Store
                // Dapr returns null if key doesn't exist
                var history = await _daprClient.GetStateAsync<string>(_stateStoreName, stateKey);

                // ... LLM inference logic would go here ...
                var response = $"Processed: {request.Message} (Context: {history ?? "No history"})";

                // 2. Update context
                var newHistory = string.IsNullOrEmpty(history) 
                    ? request.Message 
                    : $"{history}\n{request.Message}";

                // 3. Save state asynchronously
                // StateOptions can be configured for consistency/retries in production
                await _daprClient.SaveStateAsync(_stateStoreName, stateKey, newHistory);

                return Ok(new { Response = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with Dapr state store");
                return StatusCode(500, "Internal state management error");
            }
        }
    }

    // Custom Health Check for Dapr Sidecar and State Store
    public class DaprHealthCheck : IHealthCheck
    {
        private readonly DaprClient _daprClient;
        private readonly string _stateStoreName;

        public DaprHealthCheck(DaprClient daprClient, IConfiguration configuration)
        {
            _daprClient = daprClient;
            _stateStoreName = configuration.GetValue<string>("DaprStateStoreName") ?? "statestore";
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Verify Dapr sidecar is reachable and state store is responsive
                // We perform a lightweight operation (e.g., checking metadata or a dummy key)
                // For Redis, Dapr wraps the connection; checking metadata is often sufficient.
                
                // Note: DaprClient doesn't have a direct "ping" for state stores, 
                // but attempting a metadata call ensures the sidecar is up.
                await _daprClient.GetMetadataAsync(cancellationToken);
                
                return HealthCheckResult.Healthy("Dapr sidecar and state store are responsive.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Dapr sidecar or state store is unreachable.", ex);
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            
            // Register Dapr Client
            builder.Services.AddDaprClient();

            // Register Custom Health Check
            builder.Services.AddHealthChecks()
                .AddCheck<DaprHealthCheck>("dapr_state_store");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseCloudEvents();
            app.MapControllers();
            
            // Map Health Checks (exposed via /health endpoint)
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalMilliseconds
                        })
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                }
            });

            app.Run();
        }
    }
}

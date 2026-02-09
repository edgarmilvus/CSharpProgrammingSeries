
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// 1. Define the Configuration Model
// This class represents the structure of our configuration section.
// It is a Plain Old CLR Object (POCO).
public class AiServiceOptions
{
    // The name of the property must match the key in the configuration source (e.g., JSON file).
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}

// 2. Define a Service that uses the Configuration
// This service simulates calling an external AI API.
public class AiChatService
{
    private readonly ILogger<AiChatService> _logger;
    private readonly AiServiceOptions _options;

    // Inject IOptions<AiServiceOptions> to access the validated configuration
    public AiChatService(ILogger<AiChatService> logger, IOptions<AiServiceOptions> options)
    {
        _logger = logger;
        _options = options.Value; // .Value contains the strongly-typed instance
    }

    public string GenerateResponse(string prompt)
    {
        // In a real scenario, we would use _options.ApiKey here to call an external API.
        // We are simulating the logic for demonstration.
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("API Key is missing. Cannot generate response.");
        }

        _logger.LogInformation("Calling AI Endpoint: {Endpoint}", _options.Endpoint);
        _logger.LogInformation("Using API Key (first 5 chars): {KeyPrefix}...", _options.ApiKey.Substring(0, Math.Min(5, _options.ApiKey.Length)));

        return $"AI Response to '{prompt}' generated using endpoint {_options.Endpoint}.";
    }
}

// 3. Main Application Entry Point (Simulating a Console App or Web App startup)
public class Program
{
    public static void Main(string[] args)
    {
        // Create the Host builder (Standard ASP.NET Core / Generic Host pattern)
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // We are adding a JSON file source. In a real web app, appsettings.json is added by default.
                // Here we simulate it for the self-contained example.
                // Note: We don't actually write the file in code to keep it simple, 
                // but we assume a file named 'appsettings.json' exists in the execution directory.
            })
            .ConfigureServices((context, services) =>
            {
                // 4. Register the Configuration Options
                // This binds the "AiService" section from configuration to the AiServiceOptions class.
                // It ensures that the configuration is available whenever IOptions<AiServiceOptions> is requested.
                services.Configure<AiServiceOptions>(context.Configuration.GetSection("AiService"));

                // 5. Register the Service that consumes the configuration
                services.AddSingleton<AiChatService>();
            });

        var host = builder.Build();

        // --- Simulation of Runtime Execution ---
        using (var scope = host.Services.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<AiChatService>();
            
            try 
            {
                // This will trigger the configuration loading and validation logic
                string response = service.GenerateResponse("Hello, AI!");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

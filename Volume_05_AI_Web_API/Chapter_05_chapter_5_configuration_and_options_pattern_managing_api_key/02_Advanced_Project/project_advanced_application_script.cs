
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace ApiKeyManagementApp
{
    // 1. CONFIGURATION MODEL
    // We define a class to represent our configuration structure.
    // This maps directly to the JSON file and environment variables.
    // This is a Plain Old C# Object (POCO).
    public class ApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public bool IsProduction { get; set; }
    }

    // 2. SERVICE INTERFACE
    // Defines the contract for our API client logic.
    public interface IApiClient
    {
        void ConnectToService();
    }

    // 3. SERVICE IMPLEMENTATION
    // This class consumes the configuration via dependency injection.
    // It uses IOptions<ApiSettings> to access the validated settings.
    public class ApiClient : IApiClient
    {
        private readonly ApiSettings _settings;

        // Constructor Injection: We ask for IOptions<ApiSettings>
        // ASP.NET Core's Options Pattern ensures this is populated automatically.
        public ApiClient(IOptions<ApiSettings> options)
        {
            _settings = options.Value;
        }

        public void ConnectToService()
        {
            Console.WriteLine($"Attempting connection to: {_settings.ApiEndpoint}");

            // Basic validation logic within the service
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                Console.WriteLine("ERROR: Connection failed. API Key is missing.");
                return;
            }

            if (_settings.ApiKey.Length < 10)
            {
                Console.WriteLine("WARNING: API Key format looks suspicious (too short).");
            }

            // Simulate a connection check
            if (_settings.IsProduction)
            {
                Console.WriteLine("SUCCESS: Connected to Production environment using secure key.");
            }
            else
            {
                Console.WriteLine("SUCCESS: Connected to Development environment.");
            }
        }
    }

    // 4. HOSTED SERVICE
    // This runs the application logic when the console app starts.
    public class AppRunner : IHostedService
    {
        private readonly IApiClient _apiClient;

        public AppRunner(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("--- Starting API Key Management Demo ---");
            
            // Execute the core logic
            _apiClient.ConnectToService();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("--- Application Stopping ---");
            return Task.CompletedTask;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 5. CONFIGURATION BUILDER
            // We set up the configuration source hierarchy.
            // Order matters: Later sources override earlier ones (e.g., Env Vars override JSON).
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Clear default sources to be explicit (optional but good for learning)
                    config.Sources.Clear();

                    // Add appsettings.json
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    // Add Environment Variables
                    // This allows us to override keys like "ApiSettings:ApiKey" via OS env vars.
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // 6. OPTIONS PATTERN CONFIGURATION
                    // We bind the "ApiSettings" section in configuration to the ApiSettings class.
                    // This validates the structure at startup.
                    services.Configure<ApiSettings>(hostContext.Configuration.GetSection("ApiSettings"));

                    // 7. DEPENDENCY INJECTION REGISTRATION
                    // Register our interfaces and implementations.
                    services.AddSingleton<IApiClient, ApiClient>();
                    services.AddHostedService<AppRunner>();
                });

            // Build and run the host
            var host = builder.Build();
            host.Run();
        }
    }
}

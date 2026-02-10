
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

// File: Models/Configuration/AiSettings.cs
using Microsoft.Extensions.Options;

namespace AiApi.Models.Configuration
{
    public class AiSettings
    {
        public string ModelName { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
        public bool EnableGPU { get; set; }
    }

    // Implementation of validation logic
    public class AiSettingsValidator : IValidateOptions<AiSettings>
    {
        public ValidateOptionsResult Validate(string name, AiSettings options)
        {
            var failures = new List<string>();

            if (string.IsNullOrWhiteSpace(options.ModelName))
            {
                failures.Add("ModelName cannot be null or empty.");
            }

            if (options.Temperature < 0.0 || options.Temperature > 2.0)
            {
                failures.Add("Temperature must be between 0.0 and 2.0.");
            }

            if (failures.Any())
            {
                return ValidateOptionsResult.Fail(failures);
            }

            return ValidateOptionsResult.Success;
        }
    }
}

// File: Services/ChatService.cs
using AiApi.Models.Configuration;
using Microsoft.Extensions.Options;

namespace AiApi.Services
{
    public class ChatService
    {
        private readonly AiSettings _settings;

        public ChatService(IOptions<AiSettings> options)
        {
            _settings = options.Value;
        }

        public string GetConfigurationDescription()
        {
            return $"Using model {_settings.ModelName} with temperature {_settings.Temperature} and GPU enabled: {_settings.EnableGPU}.";
        }
    }
}

// File: Program.cs
using AiApi.Models.Configuration;
using AiApi.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1. Bind the configuration section
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AiSettings"));

// 2. Register the validator
// Note: We use a singleton instance for the validator
builder.Services.AddSingleton<IValidateOptions<AiSettings>, AiSettingsValidator>();

// 3. Register the service that consumes the settings
builder.Services.AddScoped<ChatService>();

var app = builder.Build();

// 4. Validation Trigger
// The Options pattern validates configuration when the IOptions<T> is first accessed.
// However, to "Fail Fast" (validate on startup), we can force validation 
// by retrieving the options monitor during startup configuration.
try
{
    using var scope = app.Services.CreateScope();
    var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AiSettings>>();
    // Accessing .CurrentValue triggers the validation logic defined in AiSettingsValidator
    var _ = options.CurrentValue; 
}
catch (OptionsValidationException ex)
{
    // Log the error and terminate the application if configuration is invalid
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical("Configuration validation failed: {Failures}", string.Join(", ", ex.Failures));
    throw; // Stop the application from starting
}

app.Run();

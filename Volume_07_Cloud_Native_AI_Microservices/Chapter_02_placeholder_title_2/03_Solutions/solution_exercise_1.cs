
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

// Project: ChatAgent.csproj
// <Project Sdk="Microsoft.NET.Sdk.Web">
//   <PropertyGroup>
//     <TargetFramework>net8.0</TargetFramework>
//   </PropertyGroup>
//   <ItemGroup>
//     <PackageReference Include="Azure.Identity" Version="1.10.0" />
//     <PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.5.0" />
//   </ItemGroup>
// </Project>

// Models/AgentConfig.cs
namespace ChatAgent.Models;
public record AgentConfig(string ModelName, double Temperature);

// Interfaces/IAgentInference.cs
using System.Threading.Tasks;
namespace ChatAgent.Interfaces;
public interface IAgentInference
{
    Task<string> GenerateResponseAsync(string prompt);
}

// Interfaces/IConfigurationProvider.cs
using ChatAgent.Models;
using System.Threading.Tasks;
namespace ChatAgent.Interfaces;
public interface IConfigurationProvider
{
    Task<AgentConfig> GetConfigAsync();
}

// Interfaces/IAzureKeyVaultClient.cs
using System.Threading.Tasks;
namespace ChatAgent.Interfaces;
public interface IAzureKeyVaultClient
{
    Task<string?> GetSecretAsync(string secretName);
}

// Implementations/MockInferenceEngine.cs
using ChatAgent.Interfaces;
using ChatAgent.Models;
using System;
using System.Threading.Tasks;
namespace ChatAgent.Implementations;
public class MockInferenceEngine : IAgentInference
{
    private readonly IConfigurationProvider _configProvider;

    public MockInferenceEngine(IConfigurationProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Simulate network delay
        await Task.Delay(500);
        
        var config = await _configProvider.GetConfigAsync();
        var processedPrompt = prompt.ToUpperInvariant();
        
        return $"Response from {config.ModelName} (Temp: {config.Temperature}): {processedPrompt}";
    }
}

// Implementations/EnvironmentConfigurationProvider.cs
using ChatAgent.Interfaces;
using ChatAgent.Models;
using System;
using System.Threading.Tasks;
namespace ChatAgent.Implementations;
public class EnvironmentConfigurationProvider : IConfigurationProvider
{
    private readonly IAzureKeyVaultClient _keyVaultClient;
    private const string DefaultModel = "gpt-4-turbo";
    private const double DefaultTemp = 0.7;

    public EnvironmentConfigurationProvider(IAzureKeyVaultClient keyVaultClient)
    {
        _keyVaultClient = keyVaultClient;
    }

    public async Task<AgentConfig> GetConfigAsync()
    {
        // 1. Try Environment Variables first
        var modelName = Environment.GetEnvironmentVariable("AI_MODEL_NAME");
        var tempStr = Environment.GetEnvironmentVariable("AI_TEMPERATURE");

        // 2. Fallback to Key Vault if missing (Secrets Injection)
        if (string.IsNullOrEmpty(modelName))
        {
            modelName = await _keyVaultClient.GetSecretAsync("AI-MODEL-NAME") ?? DefaultModel;
        }

        double temperature;
        if (string.IsNullOrEmpty(tempStr) || !double.TryParse(tempStr, out temperature))
        {
            var tempSecret = await _keyVaultClient.GetSecretAsync("AI-TEMPERATURE");
            if (!double.TryParse(tempSecret, out temperature))
            {
                temperature = DefaultTemp;
            }
        }

        return new AgentConfig(modelName, temperature);
    }
}

// Implementations/MockAzureKeyVaultClient.cs
using ChatAgent.Interfaces;
using System.Threading.Tasks;
namespace ChatAgent.Implementations;
public class MockAzureKeyVaultClient : IAzureKeyVaultClient
{
    public Task<string?> GetSecretAsync(string secretName)
    {
        // Simulate a call to Azure Key Vault
        // In a real app, this would use Azure.Security.KeyVault.Secrets
        string? secret = secretName switch
        {
            "AI-MODEL-NAME" => "production-model-1",
            "AI-TEMPERATURE" => "0.5",
            _ => null
        };
        return Task.FromResult(secret);
    }
}

// Controllers/AgentController.cs
using ChatAgent.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
namespace ChatAgent.Controllers;

[ApiController]
[Route("[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentInference _inference;

    public AgentController(IAgentInference inference)
    {
        _inference = inference;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var response = await _inference.GenerateResponseAsync(request.Prompt);
        return Ok(new { response });
    }
}

public record ChatRequest(string Prompt);

// Program.cs
using ChatAgent.Implementations;
using ChatAgent.Interfaces;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection Configuration
// Singleton: Configuration provider (rarely changes)
builder.Services.AddSingleton<IConfigurationProvider, EnvironmentConfigurationProvider>();
// Singleton: Key Vault Client (external dependency)
builder.Services.AddSingleton<IAzureKeyVaultClient, MockAzureKeyVaultClient>();
// Scoped: Inference Engine (stateless request handling)
builder.Services.AddScoped<IAgentInference, MockInferenceEngine>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

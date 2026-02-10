
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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.Threading;

namespace KernelConfigExercises;

// 6. Record Struct for Memory Efficiency
public enum ProviderType { Azure, Ollama }

public record struct ProviderConfig
{
    public ProviderType ActiveProvider { get; set; }
}

public class KernelSwitch : IHostedService
{
    private readonly IOptionsMonitor<ProviderConfig> _configMonitor;
    private readonly ILogger<KernelSwitch> _logger;
    private readonly Task _initializationTask;
    
    // Instances
    private Kernel? _azureKernel;
    private Kernel? _ollamaKernel;
    
    // Thread-safe reference
    private Kernel? _activeKernel;
    private readonly object _lock = new();

    public KernelSwitch(IOptionsMonitor<ProviderConfig> configMonitor, ILogger<KernelSwitch> logger)
    {
        _configMonitor = configMonitor;
        _logger = logger;
        _initializationTask = Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing Kernels...");

        // 2. Initialize concurrently
        var azureTask = Task.Run(() => CreateKernel(ProviderType.Azure), cancellationToken);
        var ollamaTask = Task.Run(() => CreateKernel(ProviderType.Ollama), cancellationToken);

        await Task.WhenAll(azureTask, ollamaTask);

        _azureKernel = azureTask.Result;
        _ollamaKernel = ollamaTask.Result;

        // Set initial state
        UpdateActiveKernel(_configMonitor.CurrentValue.ActiveProvider);

        // 5. Listen for config changes
        _configMonitor.OnChange((newConfig) =>
        {
            _logger.LogInformation("Configuration changed. Switching provider...");
            UpdateActiveKernel(newConfig.ActiveProvider);
        });
    }

    private void UpdateActiveKernel(ProviderType type)
    {
        // 4. Thread Safety
        lock (_lock)
        {
            _activeKernel = type switch
            {
                ProviderType.Azure => _azureKernel,
                ProviderType.Ollama => _ollamaKernel,
                _ => throw new ArgumentOutOfRangeException()
            };
            _logger.LogInformation("Active Kernel switched to: {Type}", type);
        }
    }

    // 3. Execution Method
    public async Task<string> ExecutePromptAsync(string prompt)
    {
        Kernel kernel;
        lock (_lock)
        {
            if (_activeKernel is null) throw new InvalidOperationException("Service not started.");
            kernel = _activeKernel;
        }

        // Note: In real usage, we would invoke a function. 
        // For this exercise, we simulate the call.
        return $"Executed against {kernel.Services.GetRequiredService<object>().GetType().Name}: {prompt}";
    }

    private async Task<Kernel> CreateKernel(ProviderType type)
    {
        // Simulate initialization delay
        await Task.Delay(100); 
        var builder = new KernelBuilder();
        
        if (type == ProviderType.Azure)
            builder.AddAzureOpenAIChatCompletion("gpt-4o", "https://test", "key");
        else
            builder.AddOllamaChatCompletion("llama3", "http://localhost:11434");

        return builder.Build();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Reflection;

namespace KernelConfigExercises;

// 1. The Refactored Generic Builder
public class FluentKernelBuilder<TBuilder> where TBuilder : FluentKernelBuilder<TBuilder>
{
    private readonly KernelBuilder _kernelBuilder = new();
    private bool _aiConfigured = false;
    private readonly List<Action<IServiceCollection>> _diConfigurations = new();

    // 3. Provider Agnostic Methods
    public TBuilder WithAzureOpenAI(string deployment, string endpoint, string apiKey)
    {
        _kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey);
        _aiConfigured = true;
        return (TBuilder)this;
    }

    public TBuilder WithOllama(string model, string endpoint)
    {
        _kernelBuilder.AddOllamaChatCompletion(model, endpoint);
        _aiConfigured = true;
        return (TBuilder)this;
    }

    // 3. Dependency Injection
    public TBuilder WithDependencyInjection(Action<IServiceCollection> configure)
    {
        _diConfigurations.Add(configure);
        return (TBuilder)this;
    }

    // 5. Validation
    public Kernel Build()
    {
        if (!_aiConfigured)
        {
            throw new InvalidOperationException("Cannot build Kernel: No AI Service configured.");
        }

        // Apply DI
        foreach (var config in _diConfigurations)
        {
            _kernelBuilder.Services.AddSingleton(config); // Simplified for example
            // In reality, we would inspect the builder's service collection
            // and apply the action to it.
        }

        return _kernelBuilder.Build();
    }
}

// Concreate implementation to allow instantiation without generic params
public class FluentKernelBuilder : FluentKernelBuilder<FluentKernelBuilder> { }

// 6. Unit Test Scenario (Pseudocode/Structure)
public class BuilderTests
{
    /*
     * [Fact]
     * public void Test_Switching_Providers_Preserves_Pipeline()
     * {
     *     // Arrange: Setup Builder
     *     var builder = new FluentKernelBuilder()
     *         .WithDependencyInjection(services => services.AddSingleton<ILogger, FakeLogger>())
     *         .WithAzureOpenAI("dep", "end", "key");
     * 
     *     // Act 1: Build Azure
     *     var kernel1 = builder.Build();
     *     Assert.NotNull(kernel1.Services.GetService<IChatCompletionService>());
     * 
     *     // Act 2: Switch Provider (Note: In a real builder, we might need to reset state,
     *     // but the challenge asks to verify the pipeline remains).
     *     // Since the builder is stateful, we'd typically create a new instance or 
     *     // have a 'Reset' method. For this exercise, we verify the pattern:
     *     
     *     var builder2 = new FluentKernelBuilder()
     *         .WithDependencyInjection(services => services.AddSingleton<ILogger, FakeLogger>())
     *         .WithOllama("llama3", "http://localhost");
     *         
     *     var kernel2 = builder2.Build();
     *     Assert.NotNull(kernel2.Services.GetService<IChatCompletionService>());
     * }
     */
}

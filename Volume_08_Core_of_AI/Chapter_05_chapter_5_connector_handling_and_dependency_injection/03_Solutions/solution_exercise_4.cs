
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResilienceExercise
{
    public class ResilientChatConnector : IChatCompletionService
    {
        private readonly IChatCompletionService _innerService;
        private readonly ResiliencePipeline _pipeline;

        public ResilientChatConnector(IChatCompletionService innerService, ResiliencePipeline pipeline)
        {
            _innerService = innerService;
            _pipeline = pipeline;
        }

        public IReadOnlyDictionary<string, object?> Attributes => _innerService.Attributes;

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            // Wrap the execution in the resilience pipeline
            return await _pipeline.ExecuteAsync(async token => 
                await _innerService.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, token),
                cancellationToken
            );
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    // Simulates a failing service
    public class FailingChatService : IChatCompletionService
    {
        private int _requestCount = 0;

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            _requestCount++;

            // Simulate timeout
            if (_requestCount % 5 == 0) 
            {
                // Simulate slow response
                return Task.Delay(3000, cancellationToken).ContinueWith(_ => 
                    (IReadOnlyList<ChatMessageContent>)new List<ChatMessageContent>());
            }

            // Simulate 429 or 500 errors
            if (_requestCount % 3 == 0)
            {
                throw new HttpRequestException("Service Unavailable", null, System.Net.HttpStatusCode.TooManyRequests);
            }

            return Task.FromResult<IReadOnlyList<ChatMessageContent>>(
                new List<ChatMessageContent> { new(AuthorRole.Assistant, "Success") });
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class ResilienceDemo
    {
        public static async Task Run()
        {
            // 3. Configure Resilience Pipeline
            var pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                    FailureRatio = 0.1, // Low threshold for demo
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(10),
                    OnOpened = args => 
                    {
                        Console.WriteLine($"Circuit Opened! Reason: {args.Outcome.Exception?.Message}");
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(2)) // Interactive Challenge: Timeout
                .Build();

            // Setup DI
            var services = new ServiceCollection();
            services.AddSingleton(pipeline); // Register pipeline
            services.AddTransient<IChatCompletionService, FailingChatService>();
            services.AddTransient<IChatCompletionService, ResilientChatConnector>(sp => 
                new ResilientChatConnector(
                    sp.GetRequiredService<IChatCompletionService>(), 
                    sp.GetRequiredService<ResiliencePipeline>()));

            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredService<IChatCompletionService>();

            // 6. Test Harness
            Console.WriteLine("Starting Resilience Test...");
            var tasks = new List<Task>();

            // Concurrent requests
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () => 
                {
                    try 
                    {
                        var history = new ChatHistory("Test");
                        var result = await connector.GetChatMessageContentsAsync(history);
                        Console.WriteLine($"Request succeeded: {result.FirstOrDefault()?.Content}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Request failed permanently: {ex.GetType().Name}");
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}

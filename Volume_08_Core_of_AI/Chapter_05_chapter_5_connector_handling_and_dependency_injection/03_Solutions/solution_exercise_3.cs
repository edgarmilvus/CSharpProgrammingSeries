
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiddlewareExercise
{
    // ================= APPROACH A: MIDDLEWARE (FILTERS) =================

    public class PromptSanitizerFilter : IPromptRenderFilter
    {
        public async Task InvokeAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            // Sanitize input prompt
            if (context.Prompt != null)
            {
                // Simple regex for SSN (just for demo)
                context.Prompt = Regex.Replace(context.Prompt, @"\b\d{3}-\d{2}-\d{4}\b", "***-**-****");
            }
            await next(context);
        }
    }

    public class ResponseLoggerFilter : IStreamingChatMessageContentFilter
    {
        public async Task InvokeAsync(StreamingChatMessageContentContext context, Func<StreamingChatMessageContentContext, Task> next)
        {
            await next(context);
            // Log after the response is generated
            if (context.Content != null)
            {
                Console.WriteLine($"[Audit Log]: {context.Content}");
            }
        }
    }

    // ================= APPROACH B: CUSTOM CONNECTOR WRAPPER =================

    public class SanitizedConnector : IChatCompletionService
    {
        private readonly IChatCompletionService _innerService;

        public SanitizedConnector(IChatCompletionService innerService)
        {
            _innerService = innerService;
        }

        public IReadOnlyDictionary<string, object?> Attributes => _innerService.Attributes;

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            // 1. Sanitize (Logic duplicated here)
            foreach (var message in chatHistory)
            {
                if (message.Content != null)
                    message.Content = Regex.Replace(message.Content, @"\b\d{3}-\d{2}-\d{4}\b", "***-**-****");
            }

            // 2. Call inner
            var response = await _innerService.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);

            // 3. Log
            foreach (var msg in response)
            {
                Console.WriteLine($"[Audit Log]: {msg.Content}");
            }

            return response;
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            // Implementation omitted for brevity, but would follow similar pattern
            throw new NotImplementedException();
        }
    }

    // ================= COMPARISON TEST HARNESS =================

    public class ComparisonDemo
    {
        public static void Run()
        {
            // 1. Middleware Approach
            Console.WriteLine("--- Middleware Approach ---");
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton<IChatCompletionService, MockAzureService>();
            
            // Register filters
            kernelBuilder.Services.AddSingleton<IPromptRenderFilter, PromptSanitizerFilter>();
            kernelBuilder.Services.AddSingleton<IStreamingChatMessageContentFilter, ResponseLoggerFilter>();
            
            var kernel = kernelBuilder.Build();
            
            // Note: In a real scenario, filters are automatically invoked by the Kernel when calling functions.
            // However, raw IChatCompletionService calls inside Kernel might not invoke filters unless wrapped by Kernel logic.
            // For this exercise, we assume the Kernel's pipeline handles it.

            // 2. Wrapper Approach
            Console.WriteLine("\n--- Wrapper Approach ---");
            var services = new ServiceCollection();
            services.AddTransient<IChatCompletionService, MockAzureService>();
            services.AddTransient<IChatCompletionService, SanitizedConnector>(); // Decorator
            
            var provider = services.BuildServiceProvider();
            var connector = provider.GetRequiredService<IChatCompletionService>();

            // Execute
            var history = new ChatHistory("User: 123-45-6789 is my SSN.");
            // connector.GetChatMessageContentsAsync... 
        }
    }

    public class MockAzureService : IChatCompletionService
    {
        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();
        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ChatMessageContent>>(new List<ChatMessageContent> { new(AuthorRole.Assistant, "Processed request.") });
        }
        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

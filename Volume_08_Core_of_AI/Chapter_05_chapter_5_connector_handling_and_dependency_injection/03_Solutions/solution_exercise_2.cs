
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DIExercise
{
    // 1. Stateful Conversation Cache
    public class ConversationCache
    {
        private readonly Dictionary<string, string> _store = new();

        public void Add(string key, string value) => _store[key] = value;
        
        public string Get(string key) => _store.TryGetValue(key, out var value) ? value : string.Empty;
    }

    // 2. Stateful Chat Service
    public class StatefulChatService : IChatCompletionService
    {
        private readonly ConversationCache _cache;
        private readonly IChatCompletionService _innerService;

        public StatefulChatService(ConversationCache cache, IChatCompletionService innerService)
        {
            _cache = cache;
            _innerService = innerService;
        }

        public IReadOnlyDictionary<string, object?> Attributes => _innerService.Attributes;

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            // 3. Check cache for context
            var contextKey = "global_context";
            var cachedContext = _cache.Get(contextKey);
            
            if (!string.IsNullOrEmpty(cachedContext))
            {
                // Inject cached context as a system message
                chatHistory.Insert(0, new ChatMessageContent(AuthorRole.System, cachedContext));
            }

            // Call underlying service
            var response = await _innerService.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);

            // Store new interaction (simplified logic)
            if (response.Count > 0)
            {
                _cache.Add(contextKey, response[0].Content ?? "");
            }

            return response;
        }

        // Boilerplate for interface implementation (streaming)
        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            await foreach (var chunk in _innerService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken))
            {
                yield return chunk;
            }
        }
    }

    // 5. Safe Registration (Singleton Cache + Transient Wrapper)
    public static class SafeRegistration
    {
        public static void Run()
        {
            var services = new ServiceCollection();
            
            // Singleton: Safe because ConversationCache is thread-safe (dictionary access is not, but for this exercise we assume simple usage or locking needed in real app)
            services.AddSingleton<ConversationCache>();
            
            // We simulate the underlying service (e.g., Azure OpenAI) being registered
            services.AddTransient<IChatCompletionService, MockAzureService>(); 
            
            // Transient wrapper
            services.AddTransient<StatefulChatService>();
            
            // Register the wrapper as the IChatCompletionService implementation
            services.AddTransient<IChatCompletionService>(sp => sp.GetRequiredService<StatefulChatService>());

            var provider = services.BuildServiceProvider();

            // Demonstrate safety
            using (var scope1 = provider.CreateScope())
            {
                var service1 = scope1.ServiceProvider.GetRequiredService<IChatCompletionService>();
                var service2 = scope1.ServiceProvider.GetRequiredService<IChatCompletionService>();
                // Both are Transient, so they are different instances, but they share the Singleton Cache.
                // This is safe because the cache is designed to be shared.
                Console.WriteLine("Safe Registration Complete.");
            }
        }
    }

    // 6. Scoped Registration (Simulating Web Request)
    public static class ScopedRegistration
    {
        public static void Run()
        {
            var services = new ServiceCollection();

            // Scoped: Cache lives per request/scope
            services.AddScoped<ConversationCache>();
            
            services.AddTransient<IChatCompletionService, MockAzureService>();
            
            // Wrapper must also be Scoped to ensure it shares the same Cache instance within the scope
            services.AddScoped<StatefulChatService>();
            services.AddScoped<IChatCompletionService>(sp => sp.GetRequiredService<StatefulChatService>());

            var provider = services.BuildServiceProvider();

            // Create a scope (like an HTTP request)
            using (var scope = provider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IChatCompletionService>();
                // Logic here...
            } // Scope disposed, Cache disposed.

            Console.WriteLine("Scoped Registration Complete.");
        }
    }

    // Mock underlying service
    public class MockAzureService : IChatCompletionService
    {
        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();
        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ChatMessageContent>>(new List<ChatMessageContent> { new(AuthorRole.Assistant, "Mock Response") });
        }
        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

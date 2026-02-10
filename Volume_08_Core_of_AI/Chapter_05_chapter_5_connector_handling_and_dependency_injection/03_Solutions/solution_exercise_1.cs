
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CustomConnectorExercise
{
    // 1. Define the specialized response record
    public record SpecializedLlmResponse(string GeneratedText, double ConfidenceScore, int TokenUsage);

    // 2. Simulates the raw HTTP client for the proprietary service
    public class ProprietaryLlmClient
    {
        public async Task<SpecializedLlmResponse> QueryAsync(string prompt, CancellationToken cancellationToken)
        {
            // Simulate network latency
            await Task.Delay(500, cancellationToken);
            
            // Return mock data based on the prompt
            return new SpecializedLlmResponse(
                GeneratedText: $"Processed: {prompt}",
                ConfidenceScore: 0.98,
                TokenUsage: prompt.Length / 4
            );
        }
    }

    // 3. Implements the Semantic Kernel interface
    public class ProprietaryLlmConnector : IChatCompletionService
    {
        private readonly ProprietaryLlmClient _client;

        // 4. Dependency Injection
        public ProprietaryLlmConnector(ProprietaryLlmClient client)
        {
            _client = client;
        }

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        // 5. Standard completion implementation
        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            CancellationToken cancellationToken = default)
        {
            // Map ChatHistory to proprietary prompt format (concatenation)
            var prompt = string.Join("\n", chatHistory.Select(m => $"{m.Role}: {m.Content}"));

            // Call the proprietary client
            var response = await _client.QueryAsync(prompt, cancellationToken);

            // Map back to ChatMessageContent
            var message = new ChatMessageContent(AuthorRole.Assistant, response.GeneratedText);
            
            // Handle Metadata
            message.Metadata = new Dictionary<string, object?>
            {
                ["ConfidenceScore"] = response.ConfidenceScore,
                ["TokenUsage"] = response.TokenUsage
            };

            return new List<ChatMessageContent> { message };
        }

        // 6. Streaming implementation
        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory, 
            PromptExecutionSettings? executionSettings = null, 
            Kernel? kernel = null, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Reuse the logic to get the full text first (in a real scenario, you might stream chunks from HTTP)
            var prompt = string.Join("\n", chatHistory.Select(m => $"{m.Role}: {m.Content}"));
            var response = await _client.QueryAsync(prompt, cancellationToken);

            // Simulate streaming by yielding words
            var words = response.GeneratedText.Split(' ');
            foreach (var word in words)
            {
                yield return new StreamingChatMessageContent(AuthorRole.Assistant, word + " ");
                await Task.Delay(50, cancellationToken); // Simulate token arrival delay
            }
        }
    }

    // 7. Console Application
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup DI
            var builder = Kernel.CreateBuilder();

            // Register services with appropriate lifetimes
            // Singleton: The client is stateless and expensive to create
            builder.Services.AddSingleton<ProprietaryLlmClient>();
            
            // Transient: The connector is lightweight and created per request
            builder.Services.AddTransient<IChatCompletionService, ProprietaryLlmConnector>();

            var kernel = builder.Build();

            // Retrieve the service
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            // Execute prompt
            var history = new ChatHistory();
            history.AddUserMessage("Hello, explain quantum computing.");

            Console.WriteLine("Sending prompt to Proprietary LLM...");
            var response = await chatService.GetChatMessageContentsAsync(history);

            foreach (var msg in response)
            {
                Console.WriteLine($"Assistant: {msg.Content}");
                if (msg.Metadata is not null)
                {
                    Console.WriteLine($"Confidence: {msg.Metadata["ConfidenceScore"]}");
                    Console.WriteLine($"Tokens: {msg.Metadata["TokenUsage"]}");
                }
            }
        }
    }
}

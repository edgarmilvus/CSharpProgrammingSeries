
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModularPluginExercise
{
    // 1. Plugin Interface
    public interface IConnectorPlugin
    {
        void RegisterServices(IKernelBuilder builder);
    }

    // 2. Sample Plugin (Weather)
    public class WeatherPlugin : IConnectorPlugin
    {
        public void RegisterServices(IKernelBuilder builder)
        {
            // Register a custom connector
            builder.Services.AddSingleton<IChatCompletionService, WeatherAwareConnector>();
            
            // Register native function (simplified for this exercise)
            // builder.Plugins.AddFromType<WeatherFunctions>();
        }
    }

    // Simulated Scoped Dependency (e.g., DbContext)
    public class ScopedDbContext : IDisposable
    {
        public ScopedDbContext() => Console.WriteLine("DbContext Created (Scoped)");
        public void Dispose() => Console.WriteLine("DbContext Disposed");
    }

    // The Connector that causes the bug if registered incorrectly
    public class WeatherAwareConnector : IChatCompletionService
    {
        private readonly ScopedDbContext _dbContext;

        // Depends on Scoped service
        public WeatherAwareConnector(ScopedDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            // Use the context (would fail if disposed or stale)
            return Task.FromResult<IReadOnlyList<ChatMessageContent>>(new List<ChatMessageContent> { new(AuthorRole.Assistant, "Weather data accessed.") });
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    // 3. Plugin Loader
    public class PluginLoader
    {
        public void LoadPlugins(string path, IKernelBuilder builder)
        {
            // In a real scenario, we would use AssemblyLoadContext to load DLLs from 'path'.
            // For this demo, we will just instantiate the known plugin type directly.
            
            var pluginType = typeof(WeatherPlugin);
            if (Activator.CreateInstance(pluginType) is IConnectorPlugin plugin)
            {
                plugin.RegisterServices(builder);
            }
        }
    }

    public class PluginDemo
    {
        public static void Run()
        {
            // 4. Main Application
            var builder = Kernel.CreateBuilder();
            var loader = new PluginLoader();
            
            // Load plugins
            loader.LoadPlugins("./plugins", builder);

            // --- BUG SCENARIO: Captive Dependency ---
            // If we register WeatherAwareConnector as Singleton (default in plugin RegisterServices),
            // but it depends on ScopedDbContext, we get an InvalidOperationException.
            
            // Let's simulate the fix by adjusting lifetimes.
            // The plugin registered the Connector as Singleton. We need to change it.
            
            // Remove the incorrect registration (Simulated fix)
            var descriptorsToRemove = new List<ServiceDescriptor>();
            foreach (var sd in builder.Services)
            {
                if (sd.ServiceType == typeof(IChatCompletionService) && sd.ImplementationType == typeof(WeatherAwareConnector))
                {
                    descriptorsToRemove.Add(sd);
                }
            }
            foreach (var sd in descriptorsToRemove) builder.Services.Remove(sd);

            // Re-register correctly:
            // Option A: Make Connector Scoped (matches DbContext)
            builder.Services.AddScoped<WeatherAwareConnector>();
            builder.Services.AddScoped<IChatCompletionService>(sp => sp.GetRequiredService<WeatherAwareConnector>());
            
            // Ensure DbContext is Scoped
            builder.Services.AddScoped<ScopedDbContext>();

            var kernel = builder.Build();

            // 5. Test the service within a scope (simulating a web request)
            using (var scope = kernel.Services.CreateScope())
            {
                // Resolve the service
                var chatService = scope.ServiceProvider.GetRequiredService<IChatCompletionService>();
                
                // This works because the Connector is Scoped, living as long as the scope.
                // If it were Singleton, it would hold a disposed DbContext or a stale one across requests.
                chatService.GetChatMessageContentsAsync(new ChatHistory("Test"));
            }
            
            Console.WriteLine("Scope ended. DbContext should be disposed.");
        }
    }
}


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

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup Logging
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        // Root Command
        var rootCommand = new RootCommand("Offline RAG CLI Tool");

        // --- Command 1: Index ---
        var indexCommand = new Command("index", "Ingest files into the vector database");
        var dirOption = new Argument<DirectoryInfo>("directory", "Directory containing files to index");
        var outputOption = new Option<string>(new[] { "--output", "-o" }, "Output JSONL file path") { IsRequired = true };
        var watchOption = new Option<bool>(new[] { "--watch", "-w" }, "Watch directory for changes");
        
        indexCommand.AddArgument(dirOption);
        indexCommand.AddArgument(outputOption);
        indexCommand.AddOption(watchOption);

        indexCommand.SetHandler(async (context) =>
        {
            var directory = context.ParseResult.GetValueForArgument(dirOption);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var watch = context.ParseResult.GetValueForOption(watchOption);

            if (!directory.Exists) { logger.LogError("Directory does not exist."); return; }

            var ingestor = new DocumentIngestor(); // From Ex 1
            logger.LogInformation($"Starting ingestion for {directory.FullName}...");

            if (watch)
            {
                // Interactive Challenge: Watch Mode
                logger.LogInformation("Watch mode enabled. Press Ctrl+C to stop.");
                await RunWatchMode(directory.FullName, output, ingestor, logger);
            }
            else
            {
                await ingestor.IngestDirectoryAsync(directory.FullName, output);
                logger.LogInformation("Ingestion complete.");
            }
        });

        // --- Command 2: Query ---
        var queryCommand = new Command("query", "Query the vector database and generate response");
        var questionArg = new Argument<string>("question", "The question to ask");
        var dbOption = new Option<string>(new[] { "--db" }, "Path to vector DB") { IsRequired = true };
        var modelOption = new Option<string>(new[] { "--model" }, "Path to ONNX model") { IsRequired = true };

        queryCommand.AddArgument(questionArg);
        queryCommand.AddArgument(dbOption);
        queryCommand.AddArgument(modelOption);

        queryCommand.SetHandler(async (context) =>
        {
            var question = context.ParseResult.GetValueForArgument(questionArg);
            var dbPath = context.ParseResult.GetValueForOption(dbOption);
            var modelPath = context.ParseResult.GetValueForOption(modelOption);

            try
            {
                logger.LogInformation("Loading Vector Database...");
                var db = new VectorDatabase();
                db.Load(dbPath);

                logger.LogInformation("Generating Query Embedding...");
                // Note: Using Ex 2 logic inline for brevity
                using var embedder = new EmbeddingGenerator("dummy_model.onnx"); // Placeholder path
                var queryEmbed = embedder.GenerateEmbedding(question);

                logger.LogInformation("Searching...");
                var results = db.Search(queryEmbed, k: 3, minSimilarity: 0.1f);

                if (results.Count == 0)
                {
                    logger.LogWarning("No relevant context found. Relying on internal knowledge.");
                    // Fallback: Empty context
                    results.Add(("fallback", 0.0f)); 
                }

                string context = string.Join("\n\n", results.Select(r => $"[Chunk: {r.Id}]"));
                
                logger.LogInformation("Generating Response...");
                using var llm = new LLMInferenceEngine(modelPath); // From Ex 4
                var response = llm.Generate(context, question);

                Console.WriteLine($"\nAssistant: {response}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during query processing.");
            }
        });

        rootCommand.AddCommand(indexCommand);
        rootCommand.AddCommand(queryCommand);

        await rootCommand.InvokeAsync(args);
    }

    private static async Task RunWatchMode(string directory, string output, DocumentIngestor ingestor, ILogger logger)
    {
        var tcs = new TaskCompletionSource();
        using var watcher = new FileSystemWatcher(directory);

        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.IncludeSubdirectories = true;
        watcher.Filter = "*.*";

        // Debounce logic to prevent multiple triggers on single save
        DateTime lastRun = DateTime.MinValue;
        
        watcher.Changed += async (sender, e) =>
        {
            if ((DateTime.Now - lastRun).TotalSeconds < 2) return; // Debounce
            lastRun = DateTime.Now;

            logger.LogInformation($"Detected change in {e.Name}. Updating index...");
            
            // In a real app, we would update only the changed file. 
            // For simplicity here, we re-run ingestion (incremental append logic is in Ex 1).
            await ingestor.IngestDirectoryAsync(directory, output);
            logger.LogInformation("Index updated.");
        };

        watcher.EnableRaisingEvents = true;

        // Keep the application running until interrupted
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            tcs.TrySetResult();
        };

        await tcs.Task;
    }
}

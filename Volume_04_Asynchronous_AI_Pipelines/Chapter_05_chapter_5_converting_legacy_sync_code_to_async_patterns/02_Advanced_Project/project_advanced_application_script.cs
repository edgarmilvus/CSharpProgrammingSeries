
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyAsyncMigration
{
    // ============================================================================
    // REAL-WORLD CONTEXT: LEGACY AI PIPELINE
    // ============================================================================
    // We are refactoring a legacy synchronous AI pipeline used for processing
    // customer feedback. The original system processes requests one-by-one,
    // causing significant delays (blocking I/O) when waiting for external
    // services like a Sentiment Analysis API or a Database.
    //
    // GOAL: Convert this blocking pipeline into a high-performance asynchronous
    // system that can handle multiple requests concurrently using async/await
    // and manage CPU-bound tasks via the ThreadPool.
    // ============================================================================

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Starting Legacy AI Pipeline Migration ===\n");

            // 1. Initialize the pipeline service
            var pipeline = new LegacyAIPipeline();

            // 2. Simulate incoming customer feedback requests
            // In a real scenario, these might come from an API endpoint or Message Queue.
            var feedbackRequests = new List<string>
            {
                "The user interface is intuitive and clean.",
                "I encountered a crash when uploading the file.",
                "Customer support was very helpful and fast.",
                "The pricing is too high for the features provided."
            };

            Console.WriteLine($"Received {feedbackRequests.Count} feedback items to process.\n");

            // 3. Process requests concurrently (Async Pattern)
            // We use Task.WhenAll to wait for all asynchronous operations to complete
            // without blocking the main thread unnecessarily.
            var processingTasks = new List<Task>();
            foreach (var feedback in feedbackRequests)
            {
                // We do NOT await inside the loop. This allows us to start all
                // operations immediately (concurrency).
                processingTasks.Add(pipeline.ProcessFeedbackAsync(feedback));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(processingTasks);

            Console.WriteLine("\n=== Pipeline Processing Complete ===");
        }
    }

    // ============================================================================
    // SERVICE CLASS: LEGACY AI PIPELINE
    // ============================================================================
    // This class encapsulates the logic for converting synchronous operations
    // to asynchronous ones.
    // ============================================================================
    public class LegacyAIPipeline
    {
        // Simulated external dependencies (Legacy Sync APIs)
        private readonly LegacyDatabaseService _dbService = new LegacyDatabaseService();
        private readonly LegacySentimentAnalyzer _sentimentAnalyzer = new LegacySentimentAnalyzer();

        // ============================================================================
        // METHOD: ProcessFeedbackAsync
        // ============================================================================
        // This is the core conversion method. It orchestrates the async flow.
        // ============================================================================
        public async Task ProcessFeedbackAsync(string feedback)
        {
            Console.WriteLine($"[Start] Processing: \"{feedback}\" on Thread {Thread.CurrentThread.ManagedThreadId}");

            // STEP 1: CPU-BOUND TASK (Sentiment Analysis)
            // The legacy analyzer is synchronous and CPU-intensive. We must not block
            // the main thread or event loop.
            // SOLUTION: Use Task.Run (which uses the ThreadPool) to offload this work.
            // We wrap the synchronous call in a Task to await it asynchronously.
            double sentimentScore = await Task.Run(() => _sentimentAnalyzer.Analyze(feedback));

            // STEP 2: I/O-BOUND TASK (Database Write)
            // The database service is synchronous and blocking.
            // SOLUTION: We use Task.Run again to offload the blocking I/O to a background thread.
            // This prevents the application from freezing while waiting for the database.
            bool dbSuccess = await Task.Run(() => _dbService.SaveResult(feedback, sentimentScore));

            // STEP 3: Post-Processing Logic
            if (dbSuccess && sentimentScore < 0.3)
            {
                // Another potential blocking call (e.g., sending an alert)
                await Task.Run(() => SendAlertToManager(feedback));
            }

            Console.WriteLine($"[End]   Finished processing: \"{feedback}\" on Thread {Thread.CurrentThread.ManagedThreadId}");
        }

        // Helper method for sending alerts
        private void SendAlertToManager(string feedback)
        {
            // Simulate network delay
            Thread.Sleep(100); 
            Console.WriteLine($"    -> Alert: Negative feedback detected for '{feedback.Substring(0, 10)}...'");
        }
    }

    // ============================================================================
    // LEGACY CLASS: SYNCHRONOUS DATABASE SERVICE
    // ============================================================================
    // Represents a legacy component that performs blocking I/O operations.
    // In a real migration, we cannot change the source code of this library
    // (e.g., an old ADO.NET driver), so we wrap it in Task.Run.
    // ============================================================================
    public class LegacyDatabaseService
    {
        public bool SaveResult(string feedback, double score)
        {
            // Simulate blocking network I/O (e.g., SQL INSERT)
            Thread.Sleep(200); 
            
            if (score < 0.0) throw new InvalidOperationException("Database constraint violation: Negative scores not allowed.");

            return true;
        }
    }

    // ============================================================================
    // LEGACY CLASS: SYNCHRONOUS SENTIMENT ANALYZER
    // ============================================================================
    // Represents a legacy CPU-bound library (e.g., an old ML model).
    // This blocks the thread while performing heavy calculations.
    // ============================================================================
    public class LegacySentimentAnalyzer
    {
        public double Analyze(string text)
        {
            // Simulate heavy CPU calculation
            Thread.Sleep(150);

            // Simple mock logic for sentiment scoring
            if (text.Contains("crash") || text.Contains("high")) return 0.1; // Negative
            if (text.Contains("helpful") || text.Contains("clean")) return 0.9; // Positive
            return 0.5; // Neutral
        }
    }
}


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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAIPipelineTesting
{
    // REAL-WORLD PROBLEM CONTEXT:
    // A customer support dashboard needs to process multiple user feedback messages concurrently
    // to generate sentiment analysis and priority scores. The system must handle:
    // 1. Non-deterministic AI responses (simulated via random latency and output)
    // 2. Time-bound processing (SLA of 2 seconds per message)
    // 3. Graceful degradation when AI service is slow
    // 4. Deterministic testing of business logic despite unpredictable AI behavior

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Async AI Pipeline Testing Demo ===\n");

            // Initialize components
            var mockAiService = new MockAIService();
            var feedbackProcessor = new FeedbackProcessor(mockAiService);
            var testHarness = new DeterministicTestHarness(mockAiService);

            // DEMONSTRATION 1: Production-like execution with mocked AI
            Console.WriteLine("1. PRODUCTION EXECUTION (with deterministic mock):");
            var productionResults = await feedbackProcessor.ProcessBatchAsync(
                new[] { "Love this product!", "Terrible experience", "Meh, it's okay" },
                timeoutMs: 2000
            );

            foreach (var result in productionResults)
            {
                Console.WriteLine($"   - Message: \"{result.Message}\"");
                Console.WriteLine($"     Sentiment: {result.Sentiment}, Priority: {result.Priority}");
                Console.WriteLine($"     Processed in: {result.ProcessingTimeMs}ms");
                Console.WriteLine();
            }

            // DEMONSTRATION 2: Deterministic testing of business logic
            Console.WriteLine("2. DETERMINISTIC TEST EXECUTION:");
            await testHarness.RunDeterministicTests();

            // DEMONSTRATION 3: Stress test with varying conditions
            Console.WriteLine("\n3. STRESS TEST (concurrent processing):");
            var stressResults = await feedbackProcessor.ProcessBatchAsync(
                new[] { "Urgent issue!", "Quick question", "Feature request", "Bug report" },
                timeoutMs: 1500
            );

            int successCount = 0;
            int timeoutCount = 0;
            foreach (var result in stressResults)
            {
                if (result.ProcessingTimeMs <= 1500)
                    successCount++;
                else
                    timeoutCount++;
            }
            Console.WriteLine($"   Results: {successCount} processed within SLA, {timeoutCount} timed out");
        }
    }

    // ==================== CORE BUSINESS MODELS ====================
    // These represent the data structures used throughout the pipeline
    // No advanced features - just basic classes and enums

    public enum Sentiment
    {
        Positive,
        Neutral,
        Negative
    }

    public enum Priority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class FeedbackResult
    {
        public string Message { get; set; }
        public Sentiment Sentiment { get; set; }
        public Priority Priority { get; set; }
        public int ProcessingTimeMs { get; set; }
        public bool TimedOut { get; set; }
    }

    // ==================== AI SERVICE INTERFACE ====================
    // Abstracts the non-deterministic AI behavior
    // Critical for testing: allows swapping real AI with deterministic mock

    public interface IAIService
    {
        Task<(Sentiment sentiment, Priority priority)> AnalyzeAsync(string message);
    }

    // ==================== MOCK AI SERVICE (DETERMINISTIC) ====================
    // Simulates AI behavior with controllable randomness
    // Key concept: Deterministic testing requires predictable outputs

    public class MockAIService : IAIService
    {
        private readonly Random _random;
        private readonly bool _isDeterministic;

        public MockAIService(bool isDeterministic = true)
        {
            // In production: use random for realistic behavior
            // In testing: use fixed seed for determinism
            _random = isDeterministic ? new Random(42) : new Random();
            _isDeterministic = isDeterministic;
        }

        public async Task<(Sentiment sentiment, Priority priority)> AnalyzeAsync(string message)
        {
            // Simulate variable AI latency (100-800ms)
            // In tests, this is predictable due to fixed random seed
            int latency = _random.Next(100, 800);
            await Task.Delay(latency);

            // Deterministic sentiment analysis based on keywords
            Sentiment sentiment = Sentiment.Neutral;
            if (message.Contains("Love") || message.Contains("Great"))
                sentiment = Sentiment.Positive;
            else if (message.Contains("Terrible") || message.Contains("Bug"))
                sentiment = Sentiment.Negative;

            // Priority based on sentiment and urgency keywords
            Priority priority = Priority.Medium;
            if (sentiment == Sentiment.Negative || message.Contains("Urgent") || message.Contains("Bug"))
                priority = Priority.High;
            if (message.Contains("Critical") || message.Contains("Emergency"))
                priority = Priority.Critical;

            return (sentiment, priority);
        }
    }

    // ==================== FEEDBACK PROCESSOR (BUSINESS LOGIC) ====================
    // Core pipeline that orchestrates AI calls with timeout handling
    // Demonstrates structured concurrency patterns

    public class FeedbackProcessor
    {
        private readonly IAIService _aiService;

        public FeedbackProcessor(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<List<FeedbackResult>> ProcessBatchAsync(
            string[] messages, 
            int timeoutMs)
        {
            var results = new List<FeedbackResult>();
            var tasks = new List<Task<FeedbackResult>>();

            // Create a processing task for each message
            foreach (var message in messages)
            {
                tasks.Add(ProcessSingleAsync(message, timeoutMs));
            }

            // Wait for all tasks to complete (or timeout)
            // This is "structured concurrency" - all tasks are tracked together
            var completedTasks = await Task.WhenAll(tasks);

            foreach (var result in completedTasks)
            {
                results.Add(result);
            }

            return results;
        }

        private async Task<FeedbackResult> ProcessSingleAsync(string message, int timeoutMs)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new FeedbackResult { Message = message };

            try
            // CRITICAL: Use CancellationTokenSource for timeout control
            // This allows graceful degradation when AI is slow
            using (var cts = new CancellationTokenSource(timeoutMs))
            {
                // Start the AI analysis
                var analysisTask = _aiService.AnalyzeAsync(message);

                // Create a timeout task
                var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);

                // Wait for either completion or timeout
                var completedTask = await Task.WhenAny(analysisTask, timeoutTask);

                if (completedTask == analysisTask)
                {
                    // AI responded within timeout
                    var (sentiment, priority) = await analysisTask;
                    result.Sentiment = sentiment;
                    result.Priority = priority;
                    result.TimedOut = false;
                }
                else
                {
                    // Timeout occurred
                    result.Sentiment = Sentiment.Neutral;
                    result.Priority = Priority.Low;
                    result.TimedOut = true;
                    cts.Cancel(); // Clean up
                }
            }

            stopwatch.Stop();
            result.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

            return result;
        }
    }

    // ==================== DETERMINISTIC TEST HARNESS ====================
    // Demonstrates testing async AI pipelines without flakiness
    // Key concepts: Mocking, deterministic seeds, structured assertions

    public class DeterministicTestHarness
    {
        private readonly MockAIService _mockService;

        public DeterministicTestHarness(MockAIService mockService)
        {
            _mockService = mockService;
        }

        public async Task RunDeterministicTests()
        {
            // TEST 1: Verify sentiment analysis logic
            await TestSentimentAnalysis();

            // TEST 2: Verify timeout handling
            await TestTimeoutHandling();

            // TEST 3: Verify batch processing integrity
            await TestBatchProcessing();
        }

        private async Task TestSentimentAnalysis()
        {
            Console.WriteLine("   Test: Sentiment Analysis Logic");

            // Use deterministic mock - same input always produces same output
            var (sentiment, priority) = await _mockService.AnalyzeAsync("Love this product!");

            // ASSERTION: Expected behavior based on known deterministic logic
            bool passed = sentiment == Sentiment.Positive && priority == Priority.Medium;
            
            Console.WriteLine($"     Input: \"Love this product!\"");
            Console.WriteLine($"     Expected: Positive/Medium, Got: {sentiment}/{priority}");
            Console.WriteLine($"     Result: {(passed ? "PASS" : "FAIL")}");
        }

        private async Task TestTimeoutHandling()
        {
            Console.WriteLine("   Test: Timeout Handling");

            // Create a slow mock service to trigger timeout
            var slowService = new SlowMockAIService();
            var processor = new FeedbackProcessor(slowService);

            // Process with very short timeout (50ms)
            var results = await processor.ProcessBatchAsync(
                new[] { "Test message" }, 
                timeoutMs: 50
            );

            bool passed = results[0].TimedOut == true && results[0].ProcessingTimeMs <= 60;

            Console.WriteLine($"     Timeout threshold: 50ms");
            Console.WriteLine($"     Actual processing time: {results[0].ProcessingTimeMs}ms");
            Console.WriteLine($"     Timed out: {results[0].TimedOut}");
            Console.WriteLine($"     Result: {(passed ? "PASS" : "FAIL")}");
        }

        private async Task TestBatchProcessing()
        {
            Console.WriteLine("   Test: Batch Processing Integrity");

            var processor = new FeedbackProcessor(_mockService);
            string[] testMessages = new[] { "Great!", "Bad", "Okay" };

            var results = await processor.ProcessBatchAsync(testMessages, timeoutMs: 2000);

            // ASSERTION: All messages processed, no data loss
            bool passed = results.Count == testMessages.Length;

            // ASSERTION: Each result has valid data
            foreach (var result in results)
            {
                if (string.IsNullOrEmpty(result.Message))
                {
                    passed = false;
                    break;
                }
            }

            Console.WriteLine($"     Input count: {testMessages.Length}");
            Console.WriteLine($"     Output count: {results.Count}");
            Console.WriteLine($"     All results valid: {passed}");
            Console.WriteLine($"     Result: {(passed ? "PASS" : "FAIL")}");
        }
    }

    // ==================== SLOW MOCK SERVICE (FOR TIMEOUT TESTING) ====================
    // Specialized mock that always exceeds timeout
    // Used to test timeout handling logic deterministically

    public class SlowMockAIService : IAIService
    {
        public async Task<(Sentiment sentiment, Priority priority)> AnalyzeAsync(string message)
        {
            // Always delay longer than typical timeout
            await Task.Delay(1000); // 1 second delay
            return (Sentiment.Neutral, Priority.Low);
        }
    }
}

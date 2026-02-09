
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace AsyncTestingSolutions
{
    public class ParallelPipelineTests : IAsyncLifetime
    {
        private CancellationTokenSource _cts;
        private MockAiService _mockService;

        public Task InitializeAsync()
        {
            _cts = new CancellationTokenSource();
            _mockService = new MockAiService();
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _cts.Dispose();
            await _mockService.DisposeAsync();
        }

        // 2. Define distinct Task methods
        private async Task<string> AnalyzeSentimentAsync(string input, CancellationToken token)
        {
            // Simulate work
            await Task.Delay(100, token);
            token.ThrowIfCancellationRequested();
            return $"Sentiment: {input}";
        }

        private async Task<string> ExtractEntitiesAsync(string input, CancellationToken token)
        {
            await Task.Delay(150, token);
            token.ThrowIfCancellationRequested();
            return $"Entities: {input}";
        }

        private async Task<string> SummarizeAsync(string input, CancellationToken token)
        {
            await Task.Delay(200, token);
            token.ThrowIfCancellationRequested();
            return $"Summary: {input}";
        }

        // 1. & 3. Implement the pipeline with timeout
        [Fact]
        public async Task RunParallelPipelineAsync_AllTasksSucceed_CompletesSuccessfully()
        {
            // Arrange
            _cts.CancelAfter(TimeSpan.FromSeconds(2)); // Global timeout
            var input = "Test data";

            // Act
            var tasks = new[]
            {
                AnalyzeSentimentAsync(input, _cts.Token),
                ExtractEntitiesAsync(input, _cts.Token),
                SummarizeAsync(input, _cts.Token)
            };

            // 4. & 5. Execute concurrently
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(3);
            results.Should().Contain(x => x.Contains("Sentiment"));
            results.Should().Contain(x => x.Contains("Entities"));
            results.Should().Contain(x => x.Contains("Summary"));
        }

        [Fact]
        public async Task RunParallelPipelineAsync_OneTaskFails_CancelsOthers()
        {
            // Arrange
            _cts.CancelAfter(TimeSpan.FromSeconds(2));

            // Act & Assert
            var tasks = new[]
            {
                // Simulate a failure in one task
                Task.FromException<string>(new InvalidOperationException("LLM API Down")),
                ExtractEntitiesAsync("data", _cts.Token),
                SummarizeAsync("data", _cts.Token)
            };

            // 8. Use FluentAssertions for exception handling
            var action = async () => await Task.WhenAll(tasks);
            
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("LLM API Down");

            // Verify cancellation was requested (checking internal state or verifying other tasks were cancelled)
            // Note: Task.WhenAll throws the first exception, but other tasks are cancelled via the token.
            _cts.Token.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public async Task RunParallelPipelineAsync_TimeoutExceeded_CancelsRemainingTasks()
        {
            // Arrange
            _cts.CancelAfter(TimeSpan.FromMilliseconds(50)); // Short timeout

            var tasks = new[]
            {
                AnalyzeSentimentAsync("data", _cts.Token), // 100ms delay
                ExtractEntitiesAsync("data", _cts.Token),  // 150ms delay
                SummarizeAsync("data", _cts.Token)         // 200ms delay
            };

            // Act & Assert
            // We expect a TaskCanceledException or OperationCanceledException
            var action = async () => await Task.WhenAll(tasks);
            
            await action.Should().ThrowAsync<OperationCanceledException>();
        }
    }

    // 6. IAsyncDisposable pattern for simulated resources
    public class MockAiService : IAsyncDisposable
    {
        private bool _disposed;

        public ValueTask DisposeAsync()
        {
            if (_disposed) return ValueTask.CompletedTask;
            
            // Cleanup resources (e.g., close connections)
            _disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}

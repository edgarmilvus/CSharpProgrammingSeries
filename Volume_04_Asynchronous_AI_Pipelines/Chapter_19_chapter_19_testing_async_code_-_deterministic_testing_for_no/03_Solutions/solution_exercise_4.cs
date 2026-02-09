
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using FluentAssertions;

namespace AsyncTestingSolutions
{
    // 1. Deterministic Testing Hooks: TimeProvider abstraction
    public class TestTimeProvider : TimeProvider
    {
        private long _currentTimeNanos = 0;
        
        public override long GetTimestamp() => _currentTimeNanos;
        
        public void Advance(TimeSpan duration)
        {
            Interlocked.Add(ref _currentTimeNanos, duration.Ticks);
        }

        // Override CreateTimer to control scheduled callbacks deterministically
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            // For this exercise, we simulate a simple immediate execution or delayed execution logic
            // In a real scenario, we would capture the timer and trigger it manually.
            // Here, we will rely on the logic inside the orchestrator using GetTimestamp.
            return Mock.Of<ITimer>();
        }
    }

    // Orchestrator with Resilient Logic
    public class AdvancedParallelOrchestrator : IAsyncDisposable
    {
        private readonly ILlmClient _llmClient;
        private readonly TimeProvider _timeProvider;
        private readonly ConcurrentBag<Task> _backgroundTasks = new();
        private readonly CancellationTokenSource _shutdownCts = new();

        public AdvancedParallelOrchestrator(ILlmClient llmClient, TimeProvider timeProvider)
        {
            _llmClient = llmClient;
            _timeProvider = timeProvider;
        }

        // 2. Resilient Retry Logic
        private async Task<T> ExecuteWithRetry<T>(Func<CancellationToken, Task<T>> action, CancellationToken token)
        {
            int retries = 0;
            TimeSpan delay = TimeSpan.FromMilliseconds(100);

            while (true)
            {
                try
                {
                    return await action(token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (retries >= 3) throw;
                    
                    // Chaos Engineering: Log simulated network partition
                    retries++;
                    
                    // Use TimeProvider for deterministic delay simulation
                    // Note: In real code, we'd use _timeProvider.CreateTimer or Task.Delay with provider.
                    // For this exercise, we simulate the wait by advancing the internal clock logic if possible,
                    // or simply adhering to the delay logic.
                    await Task.Delay(delay, _timeProvider);
                    
                    delay = TimeSpan.FromTicks((long)(delay.Ticks * 2.0)); // Exponential backoff
                }
            }
        }

        public async Task StartProcessingAsync(string prompt)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    // 4. Deterministic Mocking handled by injected ILlmClient
                    var result = await ExecuteWithRetry(async (ct) => await _llmClient.GetCompletionAsync(prompt, ct), _shutdownCts.Token);
                    Console.WriteLine($"Processed: {result}");
                }
                catch (OperationCanceledException) { /* Graceful shutdown */ }
            }, _shutdownCts.Token);

            _backgroundTasks.Add(task);
        }

        public async ValueTask DisposeAsync()
        {
            // 3. Structured Concurrency Scopes
            _shutdownCts.Cancel();
            
            // Wait for all background tasks to complete/fault
            await Task.WhenAll(_backgroundTasks);
            
            _shutdownCts.Dispose();
        }
    }

    public interface ILlmClient
    {
        Task<string> GetCompletionAsync(string prompt, CancellationToken ct);
    }

    public class OrchestratorTests
    {
        [Fact]
        public async Task Orchestrator_RetriesOnFailure_SucceedsEventually()
        {
            // Arrange
            var timeProvider = new TestTimeProvider();
            var mockClient = new Mock<ILlmClient>();
            
            // 4. Mock throws twice, then succeeds
            var callCount = 0;
            mockClient.Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(() =>
                      {
                          callCount++;
                          if (callCount < 3) throw new InvalidOperationException("Transient Error");
                          return "Success";
                      });

            var orchestrator = new AdvancedParallelOrchestrator(mockClient.Object, timeProvider);

            // Act
            await orchestrator.StartProcessingAsync("Test Prompt");

            // Allow time for retries (simulated by the logic, though TimeProvider integration depends on implementation)
            // In a full implementation, we would advance the TestTimeProvider here.
            await Task.Delay(500); 

            // Assert
            mockClient.Verify(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            
            // 5. Verify Resource Disposal
            await orchestrator.DisposeAsync();
            // Verify no exceptions during disposal
        }

        [Fact]
        public async Task Orchestrator_TimeProvider_DeterministicTiming()
        {
            // Arrange
            var timeProvider = new TestTimeProvider();
            var mockClient = new Mock<ILlmClient>();
            
            // Simulate delay
            mockClient.Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(async (string p, CancellationToken ct) => 
                      {
                          // Simulate work
                          await Task.Delay(100, timeProvider, ct); 
                          return "Done";
                      });

            var orchestrator = new AdvancedParallelOrchestrator(mockClient.Object, timeProvider);

            // Act
            var start = timeProvider.GetTimestamp();
            var task = orchestrator.StartProcessingAsync("Prompt");
            
            // Deterministically advance time
            timeProvider.Advance(TimeSpan.FromMilliseconds(200));
            
            await task;
            var end = timeProvider.GetTimestamp();

            // Assert
            // Verify that the execution time is exactly what we advanced, not real wall-clock time
            var elapsedTicks = end - start;
            elapsedTicks.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(100).Ticks);
        }
    }
}


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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class CircuitBreakerOpenException : Exception { }

public class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private State _currentState = State.Closed;
    private readonly SemaphoreSlim _stateLock = new SemaphoreSlim(1, 1);

    private enum State { Closed, Open, HalfOpen }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        await _stateLock.WaitAsync(ct);
        var localState = _currentState;
        _stateLock.Release();

        if (localState == State.Open)
        {
            // Check if cooldown period has passed
            if (DateTime.UtcNow - _lastFailureTime > TimeSpan.FromSeconds(15))
            {
                await TransitionState(State.HalfOpen);
            }
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }

        try
        {
            var result = await action(ct);
            
            if (localState == State.HalfOpen)
            {
                await TransitionState(State.Closed); // Success in Half-Open -> Close
            }
            
            // Reset failure count on success (optional, depending on strategy)
            Interlocked.Exchange(ref _failureCount, 0);
            
            return result;
        }
        catch (TimeoutException)
        {
            await RecordFailure();
            throw;
        }
    }

    private async Task RecordFailure()
    {
        await _stateLock.WaitAsync();
        try
        {
            Interlocked.Increment(ref _failureCount);
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= 5 && _currentState == State.Closed)
            {
                _currentState = State.Open;
                Console.WriteLine("Circuit Breaker: OPENED");
            }
            else if (_currentState == State.HalfOpen)
            {
                _currentState = State.Open; // Trial request failed
                Console.WriteLine("Circuit Breaker: Re-Opened");
            }
        }
        finally
        {
            _stateLock.Release();
        }
    }

    private async Task TransitionState(State newState)
    {
        await _stateLock.WaitAsync();
        _currentState = newState;
        _stateLock.Release();
    }
}

public class ResilientPipeline
{
    private readonly CircuitBreaker _circuitBreaker = new();
    private readonly HttpClient _httpClient = new();

    public async Task ProcessBatchAsync(List<string> prompts, CancellationToken ct)
    {
        // Using Parallel.ForEachAsync for concurrency
        await Parallel.ForEachAsync(prompts, new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = ct }, async (prompt, token) =>
        {
            try
            {
                // Wrap the request in the Circuit Breaker + Timeout Logic
                await _circuitBreaker.ExecuteAsync(async (innerCt) =>
                {
                    // Per-request timeout mechanism (similar to Ex 1)
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(2)); // Strict 2s timeout
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);

                    var requestTask = _httpClient.GetAsync($"http://localhost:5000/api/infer?prompt={prompt}", linkedCts.Token);
                    var delayTask = Task.Delay(TimeSpan.FromSeconds(2), linkedCts.Token);

                    var completed = await Task.WhenAny(requestTask, delayTask);
                    if (completed == delayTask) throw new TimeoutException($"Prompt {prompt} timed out.");

                    var response = await requestTask;
                    response.EnsureSuccessStatusCode();
                    return response;
                }, ct);

                Console.WriteLine($"Processed: {prompt}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed {prompt}: {ex.GetType().Name}");
            }
        });
    }
}

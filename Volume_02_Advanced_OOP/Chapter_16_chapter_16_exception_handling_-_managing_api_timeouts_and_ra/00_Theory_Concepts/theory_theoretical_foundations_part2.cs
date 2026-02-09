
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AI.Resilience
{
    // Delegate definition for a retry strategy
    public delegate Task<TimeSpan> RetryStrategy(int attemptCount, Exception ex);

    public class ExponentialBackoffStrategy
    {
        private readonly TimeSpan _baseDelay;
        private readonly TimeSpan _maxDelay;
        private readonly double _jitterFactor;

        public ExponentialBackoffStrategy(TimeSpan baseDelay, TimeSpan maxDelay, double jitterFactor = 0.1)
        {
            _baseDelay = baseDelay;
            _maxDelay = maxDelay;
            _jitterFactor = jitterFactor;
        }

        // Implementation of the strategy as a lambda expression
        // Lambdas allow us to define inline anonymous functions, perfect for encapsulating stateful logic
        public RetryStrategy GetRetryLogic()
        {
            return async (attemptCount, ex) =>
            {
                // Calculate exponential delay
                double delayMs = _baseDelay.TotalMilliseconds * Math.Pow(2, attemptCount);
                
                // Cap the delay at the maximum threshold
                if (delayMs > _maxDelay.TotalMilliseconds)
                    delayMs = _maxDelay.TotalMilliseconds;

                // Add jitter (randomness) to desynchronize retries
                var random = new Random();
                var jitter = delayMs * _jitterFactor * (random.NextDouble() * 2 - 1);
                var finalDelay = Math.Max(0, delayMs + jitter);

                return TimeSpan.FromMilliseconds(finalDelay);
            };
        }
    }
}

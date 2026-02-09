
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AI.Exceptions
{
    // Base class for all AI-related operational exceptions
    public abstract class AIOperationException : Exception
    {
        public AIOperationException(string message) : base(message) { }
        public AIOperationException(string message, Exception inner) : base(message, inner) { }
    }

    // Represents a transient failure where a retry is appropriate
    public class TransientFailureException : AIOperationException
    {
        public TransientFailureException(string message, Exception inner) : base(message, inner) { }
    }

    // Specific to HTTP 429 (Too Many Requests) or provider-specific rate limits
    public class RateLimitExceededException : TransientFailureException
    {
        public TimeSpan? RetryAfter { get; set; }

        public RateLimitExceededException(string message, TimeSpan? retryAfter, Exception inner) 
            : base(message, inner)
        {
            RetryAfter = retryAfter;
        }
    }

    // Specific to network timeouts
    public class APITimeoutException : TransientFailureException
    {
        public APITimeoutException(string message, Exception inner) : base(message, inner) { }
    }
}


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

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter16.Exercise1
{
    // 1. Define the base class
    public class AIProviderException : Exception
    {
        public AIProviderException(string message) : base(message) { }
        public AIProviderException(string message, Exception inner) : base(message, inner) { }
    }

    // 2. Define derived classes
    public class RateLimitExceededException : AIProviderException
    {
        public int RetryAfterSeconds { get; }

        public RateLimitExceededException(int retryAfterSeconds) 
            : base($"Rate limit exceeded. Retry after {retryAfterSeconds} seconds.")
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }

    public class AuthenticationFailedException : AIProviderException
    {
        public AuthenticationFailedException(string message) : base(message) { }
    }

    public static class ApiErrorParser
    {
        // 3. Static helper method to throw based on status code
        public static void ThrowFromStatusCode(int statusCode, string responseBody)
        {
            if (statusCode == 429)
            {
                // Simulate parsing "Retry-After" from the body for this exercise
                // In a real scenario, this usually comes from Headers["Retry-After"]
                int retryAfter = 60; // Default fallback
                if (int.TryParse(responseBody, out int parsed))
                {
                    retryAfter = parsed;
                }
                
                throw new RateLimitExceededException(retryAfter);
            }
            
            if (statusCode == 401 || statusCode == 403)
            {
                throw new AuthenticationFailedException($"Authentication failed with status code {statusCode}.");
            }

            // Standard exception for other errors
            throw new HttpRequestException($"Request failed with status code {statusCode}");
        }
    }
}

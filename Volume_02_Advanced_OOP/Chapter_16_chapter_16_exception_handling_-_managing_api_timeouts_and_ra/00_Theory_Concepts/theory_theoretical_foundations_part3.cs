
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

// Source File: theory_theoretical_foundations_part3.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.Net.Http;
using System.Threading.Tasks;
using AI.Exceptions;
using AI.Resilience;

namespace AI.Clients
{
    public class ResilientAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly RetryStrategy _retryStrategy;

        public ResilientAIClient(HttpClient httpClient, RetryStrategy retryStrategy)
        {
            _httpClient = httpClient;
            _retryStrategy = retryStrategy;
        }

        public async Task<string> QueryModelAsync(string prompt)
        {
            int attemptCount = 0;
            
            while (true)
            {
                try
                {
                    // Simulate an API call
                    var response = await _httpClient.GetAsync($"https://api.example.ai/query?prompt={prompt}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                    // Handle specific HTTP status codes
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        // Parse Retry-After header if available
                        TimeSpan? retryAfter = null; 
                        if (response.Headers.TryGetValues("Retry-After", out var values))
                        {
                            retryAfter = TimeSpan.FromSeconds(int.Parse(values.First()));
                        }
                        
                        throw new RateLimitExceededException("Rate limit hit.", retryAfter, null);
                    }

                    // Treat other 4xx/5xx as transient for this example
                    throw new TransientFailureException($"HTTP {response.StatusCode}", null);
                }
                catch (RateLimitExceededException ex) when (ex.RetryAfter.HasValue)
                {
                    // If the provider explicitly asks for a wait time, honor it over the backoff strategy
                    await Task.Delay(ex.RetryAfter.Value);
                    attemptCount++;
                }
                catch (TransientFailureException ex)
                {
                    attemptCount++;
                    
                    // Use the delegate to calculate the delay
                    // This decouples the retry logic from the specific delay algorithm
                    TimeSpan delay = await _retryStrategy(attemptCount, ex);
                    
                    await Task.Delay(delay);
                }
                catch (HttpRequestException ex)
                {
                    // Network level failure (DNS, connection refused)
                    // Treat as transient but wrap specifically
                    throw new APITimeoutException("Network failure during API call.", ex);
                }
            }
        }
    }
}


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

namespace AIChatRateLimitingSimulation
{
    // Core Domain: Represents a user interacting with our AI Chat API.
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int TokenBalance { get; set; } // Simulates computational budget (e.g., GPT-4 tokens)
    }

    // Core Domain: Represents an AI Chat Request.
    public class ChatRequest
    {
        public string UserId { get; set; }
        public string Message { get; set; }
        public int EstimatedTokenCost { get; set; } // High latency operation cost
    }

    // Core Concept: Rate Limiter Interface (Abstracting the strategy pattern).
    public interface IRateLimiter
    {
        bool IsAllowed(ChatRequest request);
        string GetLimitDescription();
    }

    // Strategy 1: Fixed Window Counter Algorithm.
    // Resets the count completely after a fixed time interval (e.g., per minute).
    public class FixedWindowLimiter : IRateLimiter
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _window;
        private DateTime _windowStart;
        private int _currentCount;

        public FixedWindowLimiter(int maxRequests, TimeSpan window)
        {
            _maxRequests = maxRequests;
            _window = window;
            _windowStart = DateTime.UtcNow;
            _currentCount = 0;
        }

        public bool IsAllowed(ChatRequest request)
        {
            lock (this) // Thread safety for concurrent access
            {
                DateTime now = DateTime.UtcNow;
                
                // Check if window has expired
                if (now - _windowStart >= _window)
                {
                    _windowStart = now;
                    _currentCount = 0;
                }

                if (_currentCount < _maxRequests)
                {
                    _currentCount++;
                    return true;
                }

                return false;
            }
        }

        public string GetLimitDescription()
        {
            return $"Fixed Window: {_maxRequests} requests per {_window.TotalSeconds} seconds";
        }
    }

    // Strategy 2: Token Bucket Algorithm.
    // Tokens refill at a steady rate. Requests consume tokens.
    // Allows for bursting traffic (accumulated tokens).
    public class TokenBucketLimiter : IRateLimiter
    {
        private readonly int _bucketCapacity;
        private readonly double _refillRatePerSecond; // Tokens per second
        private double _currentTokens;
        private DateTime _lastRefillTime;

        public TokenBucketLimiter(int capacity, double refillRate)
        {
            _bucketCapacity = capacity;
            _refillRatePerSecond = refillRate;
            _currentTokens = capacity;
            _lastRefillTime = DateTime.UtcNow;
        }

        public bool IsAllowed(ChatRequest request)
        {
            lock (this)
            {
                RefillTokens();

                // Cost is proportional to the estimated token usage of the AI model
                int cost = request.EstimatedTokenCost; 

                if (_currentTokens >= cost)
                {
                    _currentTokens -= cost;
                    return true;
                }

                return false;
            }
        }

        private void RefillTokens()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan timePassed = now - _lastRefillTime;
            double tokensToAdd = timePassed.TotalSeconds * _refillRatePerSecond;

            if (tokensToAdd > 0)
            {
                _currentTokens = Math.Min(_bucketCapacity, _currentTokens + tokensToAdd);
                _lastRefillTime = now;
            }
        }

        public string GetLimitDescription()
        {
            return $"Token Bucket: Capacity {_bucketCapacity}, Refills {_refillRatePerSecond}/sec";
        }
    }

    // Core Service: Orchestrates the request flow using the configured limiter.
    public class AIChatService
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly Dictionary<string, User> _users;

        public AIChatService(IRateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter;
            _users = new Dictionary<string, User>
            {
                { "u1", new User { Id = "u1", Name = "Alice", TokenBalance = 1000 } },
                { "u2", new User { Id = "u2", Name = "Bob", TokenBalance = 500 } }
            };
        }

        public void ProcessRequest(ChatRequest request)
        {
            Console.WriteLine($"[Inbound] User: {request.UserId}, Tokens: {request.EstimatedTokenCost}");

            // 1. Apply Rate Limiting Logic
            if (!_rateLimiter.IsAllowed(request))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Rejected] Rate limit exceeded. Limit: {_rateLimiter.GetLimitDescription()}");
                Console.ResetColor();
                return;
            }

            // 2. Check User Balance (Business Logic)
            if (!_users.ContainsKey(request.UserId))
            {
                Console.WriteLine("[Error] User not found.");
                return;
            }

            var user = _users[request.UserId];
            if (user.TokenBalance < request.EstimatedTokenCost)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Rejected] Insufficient token balance. User has {user.TokenBalance}, needs {request.EstimatedTokenCost}.");
                Console.ResetColor();
                return;
            }

            // 3. Execute AI Inference (Simulated)
            user.TokenBalance -= request.EstimatedTokenCost;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Success] AI Response generated. User balance remaining: {user.TokenBalance}");
            Console.ResetColor();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Simulation 1: Fixed Window Limiter (Strict Limit) ---");
            // Scenario: Strict limit of 3 requests per 5 seconds.
            var fixedLimiter = new FixedWindowLimiter(3, TimeSpan.FromSeconds(5));
            var chatServiceFixed = new AIChatService(fixedLimiter);
            
            SimulateTraffic(chatServiceFixed, "u1");

            Console.WriteLine("\n--- Simulation 2: Token Bucket Limiter (Burst & Sustained) ---");
            // Scenario: Capacity of 10 tokens, refills 2 tokens/sec. Cost is 2 per request.
            var tokenLimiter = new TokenBucketLimiter(10, 2.0);
            var chatServiceToken = new AIChatService(tokenLimiter);

            SimulateTraffic(chatServiceToken, "u2");
        }

        // Helper method to simulate bursty traffic patterns
        static void SimulateTraffic(AIChatService service, string userId)
        {
            // Burst: 4 requests immediately
            for (int i = 0; i < 4; i++)
            {
                service.ProcessRequest(new ChatRequest { UserId = userId, Message = "Hello", EstimatedTokenCost = 2 });
                Thread.Sleep(100); // Small delay between requests
            }

            // Wait for refill
            Console.WriteLine("[Waiting] 3 seconds for potential refill...");
            Thread.Sleep(3000);

            // Sustained: 1 request
            service.ProcessRequest(new ChatRequest { UserId = userId, Message = "Hello again", EstimatedTokenCost = 2 });
        }
    }
}

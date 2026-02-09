
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChatGPTClone.Backend.Core
{
    // Real-World Problem: 
    // A startup is building a lightweight "ChatGPT-Clone" backend API.
    // To reduce costs and latency, they implement a "Mock AI Service" that simulates
    // token-by-token streaming responses (like a real LLM) before integrating expensive cloud APIs.
    // This console application simulates the core backend logic: 
    // 1. Handling User Authentication (JWT simulation)
    // 2. Managing Conversation State (History)
    // 3. Generating Streaming Responses (Simulated AI)
    // 4. Logging requests for audit trails.

    class Program
    {
        // In-memory database simulation (replacing SQL for this console demo)
        static List<Conversation> _conversations = new List<Conversation>();
        static int _conversationIdCounter = 1;

        static void Main(string[] args)
        {
            Console.WriteLine("=== ChatGPT-Clone Backend API Simulator ===");
            Console.WriteLine("Type 'exit' to stop the application.\n");

            // Simulate a logged-in user context
            string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock.token";
            string currentUserId = "user_123";

            while (true)
            {
                Console.Write("User Input: ");
                string userInput = Console.ReadLine();

                if (userInput.ToLower() == "exit") break;

                // 1. Validate Input
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Error: Input cannot be empty.\n");
                    continue;
                }

                // 2. Authenticate Request (Simulated Middleware)
                if (!IsAuthenticated(jwtToken))
                {
                    Console.WriteLine("401 Unauthorized: Invalid Token.\n");
                    continue;
                }

                // 3. Process Chat Request
                try
                {
                    ProcessChatRequest(currentUserId, userInput);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"500 Internal Server Error: {ex.Message}\n");
                }

                Console.WriteLine(); // Visual separator
            }
        }

        // Simulates ASP.NET Core Middleware for Authentication
        static bool IsAuthenticated(string token)
        {
            // In a real app, we would decode the JWT and validate the signature.
            // Here, we just check if the token is not null or empty.
            return !string.IsNullOrEmpty(token) && token.Contains("mock");
        }

        // Orchestrates the request flow (Controller + Service Layer logic)
        static void ProcessChatRequest(string userId, string message)
        {
            // Find or create conversation
            Conversation conversation = GetOrCreateConversation(userId);

            // Add user message to history
            conversation.History.Add($"User: {message}");

            // Log the request (Audit Trail)
            LogRequest($"User {userId} sent message to Conversation {conversation.Id}");

            // Generate AI Response (Streaming Simulation)
            string aiResponse = GenerateStreamingResponse(message);

            // Add AI response to history
            conversation.History.Add($"AI: {aiResponse}");

            // Persist (Simulated Database Save)
            SaveConversation(conversation);

            // Output result
            Console.WriteLine($"AI Response: {aiResponse}");
        }

        // Simulates a Database Lookup/Creation
        static Conversation GetOrCreateConversation(string userId)
        {
            // Basic linear search (no LINQ allowed per constraints)
            for (int i = 0; i < _conversations.Count; i++)
            {
                if (_conversations[i].UserId == userId)
                {
                    return _conversations[i];
                }
            }

            // Create new if not found
            var newConv = new Conversation
            {
                Id = _conversationIdCounter++,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                History = new List<string>()
            };
            _conversations.Add(newConv);
            return newConv;
        }

        // Simulates the AI Service generating a response token-by-token
        static string GenerateStreamingResponse(string userMessage)
        {
            StringBuilder responseBuilder = new StringBuilder();
            
            Console.Write("AI is typing: ");

            // Simulate processing time and token generation
            string[] tokens = GetMockTokens(userMessage);

            foreach (string token in tokens)
            {
                // Simulate network latency (streaming delay)
                System.Threading.Thread.Sleep(150); 
                
                // Append token to response
                responseBuilder.Append(token);
                
                // In a real Web API, this would flush to the HTTP response stream.
                // In Console, we simulate the visual effect.
                Console.Write(token + " ");
            }
            Console.WriteLine("\n"); // End of stream

            return responseBuilder.ToString();
        }

        // Logic to determine response based on input (Mock AI Brain)
        static string[] GetMockTokens(string input)
        {
            if (input.ToLower().Contains("hello"))
            {
                return new string[] { "Hello", "there!", "How", "can", "I", "assist", "you", "today?" };
            }
            else if (input.ToLower().Contains("error"))
            {
                return new string[] { "I", "sense", "a", "problem.", "Let's", "debug", "this", "together." };
            }
            else
            {
                return new string[] { "I", "received", "your", "message:", input, "- Generating", "response", "logic..." };
            }
        }

        // Simulates saving to a database (e.g., SQL Server or CosmosDB)
        static void SaveConversation(Conversation conv)
        {
            // In a real app, we would use Entity Framework Core here.
            // We check if the conversation exists in our list and update it.
            for (int i = 0; i < _conversations.Count; i++)
            {
                if (_conversations[i].Id == conv.Id)
                {
                    _conversations[i] = conv;
                    // Console.WriteLine($"[DEBUG] Database updated for Conversation ID: {conv.Id}");
                    return;
                }
            }
        }

        // Simulates a logging service (Serilog/NLog)
        static void LogRequest(string message)
        {
            string logEntry = $"[{DateTime.UtcNow.ToString("HH:mm:ss")}] INFO: {message}";
            
            // Append to a text file to simulate persistence
            try
            {
                File.AppendAllText("audit_log.txt", logEntry + Environment.NewLine);
            }
            catch (Exception)
            {
                // Fail silently for demo purposes, or fallback to console
                Console.WriteLine($"[Log Failover] {logEntry}");
            }
        }
    }

    // Domain Model (Entity)
    // Represents a Chat Session
    public class Conversation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> History { get; set; } // Stores the chat thread
    }
}

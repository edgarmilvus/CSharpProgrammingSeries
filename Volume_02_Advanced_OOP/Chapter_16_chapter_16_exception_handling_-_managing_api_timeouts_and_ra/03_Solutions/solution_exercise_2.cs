
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
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter16.Exercise2
{
    public class APISession : IDisposable
    {
        private readonly HttpClient _httpClient;

        public APISession()
        {
            _httpClient = new HttpClient();
            Console.WriteLine("Session started. HttpClient initialized.");
        }

        // Accepts a lambda expression (Func<string, string>)
        public string ExecuteRequest(Func<string, string> requestGenerator)
        {
            try
            {
                // Simulate a URL
                string url = "https://api.example.com/resource";
                
                // Execute the lambda passed in
                string result = requestGenerator(url);
                
                Console.WriteLine("Request executed successfully.");
                return result;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error detected in session: {ex.Message}");
                // Re-throw as per instructions
                throw;
            }
        }

        public void Dispose()
        {
            // Cleanup resources
            _httpClient?.Dispose();
            Console.WriteLine("Session resources released.");
        }
    }

    // Usage Example (for context):
    public class Program
    {
        public static void Run()
        {
            // The 'using' statement ensures Dispose() is called automatically
            using (var session = new APISession())
            {
                try 
                {
                    // Passing a Lambda Expression
                    string response = session.ExecuteRequest(url => 
                    {
                        // Simulate logic that might throw
                        if (url.Contains("error")) throw new Exception("Simulated Error");
                        return "{ 'data': 'success' }";
                    });
                }
                catch { /* Catching the re-thrown exception */ }
            }
        }
    }
}

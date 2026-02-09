
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

# Source File: solution_exercise_7.cs
# Description: Solution for Exercise 7
# ==========================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LegacyPlugin.Pages
{
    [IgnoreAntiforgeryToken]
    public class ChatModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "chat_context_";

        public ChatModel(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        [BindProperty]
        public string UserPrompt { get; set; }

        public string ResponseText { get; set; }

        public async Task OnGetAsync()
        {
            // Initial load
        }

        public async Task OnPostAsync()
        {
            var sessionId = HttpContext.Session.Id; // Or a custom user ID
            var contextKey = CacheKeyPrefix + sessionId;

            // 1. Simulate LLM Intent Recognition (Heuristic)
            bool isLookupRequest = UserPrompt.Contains("email") || UserPrompt.Contains("details") || UserPrompt.Contains("lookup");
            
            if (isLookupRequest)
            {
                // 2. Context Retrieval (Stateful Challenge)
                string employeeId = null;

                // Check if user asked for "their email" (follow-up)
                if (UserPrompt.Contains("their") || UserPrompt.Contains("email"))
                {
                    _cache.TryGetValue(contextKey, out employeeId);
                }
                
                // If explicit ID provided in prompt, extract it (simplified regex for demo)
                if (employeeId == null)
                {
                    // Fallback logic: assume ID is "123" for demo purposes if not in context
                    employeeId = "123"; 
                }

                // 3. Call Plugin Backend
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sk-1234567890");

                var payload = new
                {
                    Calls = new[]
                    {
                        new { FunctionName = "lookup_employee", Arguments = new { id = employeeId } }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5000/api/plugins/execute", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    // Parse result (simplified for brevity)
                    var result = JsonDocument.Parse(resultJson);
                    var output = result.RootElement.GetProperty("results")[0].GetProperty("Output").GetString();

                    // 4. Update Context Cache
                    _cache.Set(contextKey, employeeId, TimeSpan.FromMinutes(10));

                    // 5. Format Natural Language Response
                    ResponseText = $"[System Context: ID {employeeId} cached]\n\nLLM Simulation: Based on the plugin response: {output}";
                }
                else
                {
                    ResponseText = "Error calling plugin: " + response.StatusCode;
                }
            }
            else
            {
                ResponseText = "I can only help with employee lookups right now.";
            }
        }
    }
}

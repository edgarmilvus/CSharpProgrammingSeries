
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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace AIApiClient
{
    // 1. REAL-WORLD CONTEXT:
    // Imagine a Customer Support Dashboard for an e-commerce platform. 
    // Support agents need a tool to quickly summarize customer feedback tickets.
    // The tool must be resilient to network issues (common in enterprise environments)
    // and efficient in managing HTTP connections to avoid socket exhaustion when
    // processing hundreds of tickets daily.

    class Program
    {
        // 2. ENTRY POINT:
        // We simulate the application startup and dependency configuration.
        // In a real ASP.NET Core app, HttpClientFactory and Polly policies 
        // would be registered in Startup.cs or Program.cs using Dependency Injection.
        // Here, we manually instantiate the client to demonstrate the mechanics.
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initializing AI Support Summarizer...\n");

            // Configuration: In production, these come from IConfiguration or Azure Key Vault.
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "sk-mock-key-for-demo";
            string endpoint = "https://api.openai.com/v1/chat/completions"; // Azure OpenAI endpoint structure is similar

            // Instantiate our typed service. 
            // We pass a pre-configured HttpClient (simulating IHttpClientFactory behavior).
            // We also configure the resilience policy (Polly) inside the service constructor.
            var summarizerService = new TicketSummarizerService(apiKey, endpoint);

            // Sample Data: Simulating a batch of customer tickets retrieved from a database.
            string[] tickets = new string[3]
            {
                "Customer #101: I ordered a blue shirt but received red. The delivery was late by 2 days. I want a refund immediately.",
                "Customer #102: The website crashes on checkout when using Safari. Tried multiple times. Very frustrating.",
                "Customer #103: Love the new headphones! Battery life is amazing. However, the noise cancellation could be better."
            };

            Console.WriteLine($"Processing {tickets.Length} tickets...\n");

            // Process tickets sequentially (simulating a background worker).
            // We use basic loops and conditionals as requested.
            for (int i = 0; i < tickets.Length; i++)
            {
                string currentTicket = tickets[i];
                Console.WriteLine($"[Ticket {i + 1}] Original: {currentTicket}");

                try
                {
                    // Call the AI service with resilience handling built-in.
                    string summary = await summarizerService.SummarizeTicketAsync(currentTicket);
                    Console.WriteLine($"[Summary]: {summary}\n");
                }
                catch (Exception ex)
                {
                    // Handle catastrophic failures (e.g., API down, invalid auth).
                    Console.WriteLine($"[Error]: Failed to process ticket. Details: {ex.Message}\n");
                }

                // Simulate delay to respect rate limits (basic throttling).
                await Task.Delay(1000); 
            }

            Console.WriteLine("Batch processing complete.");
        }
    }

    // 3. ARCHITECTURAL COMPONENT: REQUEST BUILDER
    // Implements the Builder Pattern to construct the JSON payload.
    // This isolates the complexity of serialization and structure from the service logic.
    public class OpenAiRequestBuilder
    {
        private string _model;
        private string _systemPrompt;
        private string _userPrompt;

        public OpenAiRequestBuilder SetModel(string model)
        {
            _model = model;
            return this;
        }

        public OpenAiRequestBuilder SetSystemPrompt(string prompt)
        {
            _systemPrompt = prompt;
            return this;
        }

        public OpenAiRequestBuilder SetUserPrompt(string prompt)
        {
            _userPrompt = prompt;
            return this;
        }

        // Builds the raw JSON string.
        // We avoid using System.Text.Json source generators or Records here 
        // to stick to explicit object construction.
        public string Build()
        {
            // Structure: { "model": "...", "messages": [ { "role": "system", "content": "..." }, { "role": "user", "content": "..." } ] }
            // We construct this manually via string concatenation for transparency, 
            // though in production we would use JsonSerializer.Serialize on a defined class.
            
            string json = "{";
            json += $"\"model\": \"{_model}\",";
            json += "\"messages\": [";
            
            // System Message
            json += "{";
            json += $"\"role\": \"system\",";
            json += $"\"content\": \"{_systemPrompt}\"";
            json += "},";

            // User Message
            json += "{";
            json += $"\"role\": \"user\",";
            json += $"\"content\": \"{_userPrompt}\"";
            json += "}";
            
            json += "]";
            json += "}";

            return json;
        }
    }

    // 4. ARCHITECTURAL COMPONENT: TYPED SERVICE
    // This class encapsulates the HttpClient, Authentication, and Resilience Logic.
    // It represents the core consumption logic taught in Chapter 11.
    public class TicketSummarizerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        
        // Resilience Configuration (Simulating Polly)
        private const int MaxRetries = 3;
        private const int InitialDelayMs = 1000;

        public TicketSummarizerService(string apiKey, string endpoint)
        {
            _apiKey = apiKey;

            // CRITICAL: Managing HttpClient Lifecycle
            // In a real app using IHttpClientFactory, we would inject IHttpClientFactory here
            // and create a client named "OpenAI" with pre-configured BaseAddress and DefaultRequestHeaders.
            // To simulate this without DI container, we instantiate HttpClient manually.
            // NOTE: In production console apps, reuse this HttpClient instance. Do not dispose it per request.
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(endpoint);
            
            // Authentication Setup (API Key)
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _apiKey);
            
            // Headers for JSON content
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> SummarizeTicketAsync(string ticketContent)
        {
            // 1. Request Construction using Builder Pattern
            var builder = new OpenAiRequestBuilder();
            string payload = builder
                .SetModel("gpt-3.5-turbo") // Or "gpt-4" or Azure deployment name
                .SetSystemPrompt("You are a support assistant. Summarize the customer issue in one concise sentence.")
                .SetUserPrompt(ticketContent)
                .Build();

            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

            // 2. Resilience Loop (Simulating Polly's WaitAndRetry)
            int retryCount = 0;
            while (true)
            {
                try
                {
                    // 3. Execution
                    HttpResponseMessage response = await _httpClient.PostAsync("", httpContent);

                    // 4. Success Handling
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return ParseSummaryFromJson(responseBody);
                    }
                    
                    // 5. Transient Fault Handling (Retry Logic)
                    // We check for specific status codes that warrant a retry (5xx, 429).
                    int statusCode = (int)response.StatusCode;
                    if (retryCount < MaxRetries && 
                       (statusCode >= 500 || statusCode == 429)) // 429 = Too Many Requests
                    {
                        retryCount++;
                        // Exponential Backoff Calculation
                        int delay = InitialDelayMs * (int)Math.Pow(2, retryCount - 1);
                        Console.WriteLine($"  -> Warning: API returned {statusCode}. Retrying in {delay}ms (Attempt {retryCount}/{MaxRetries})...");
                        await Task.Delay(delay);
                        continue; 
                    }

                    // Non-retryable error (e.g., 401 Unauthorized, 400 Bad Request)
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API Error {statusCode}: {errorContent}");
                }
                catch (HttpRequestException ex)
                {
                    // Network errors (timeouts, DNS failures)
                    if (retryCount < MaxRetries)
                    {
                        retryCount++;
                        int delay = InitialDelayMs * (int)Math.Pow(2, retryCount - 1);
                        Console.WriteLine($"  -> Warning: Network error ({ex.Message}). Retrying in {delay}ms...");
                        await Task.Delay(delay);
                        continue;
                    }
                    throw; // Rethrow if retries exhausted
                }
            }
        }

        // 6. RESPONSE PARSING
        // Manual JSON parsing logic to avoid external dependencies and demonstrate structure understanding.
        // In production, we would deserialize into a C# DTO class.
        private string ParseSummaryFromJson(string json)
        {
            // We are looking for: "content": "Summarized text..."
            // A simple string search is used here to avoid complex parsing logic 
            // and keep the code within basic blocks constraint.
            string searchKey = "\"content\": \"";
            int startIndex = json.IndexOf(searchKey);
            if (startIndex == -1) return "Error parsing response";

            startIndex += searchKey.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1) return "Error parsing response";

            return json.Substring(startIndex, endIndex - startIndex);
        }
    }
}

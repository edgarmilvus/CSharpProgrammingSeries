
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

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

// 1. Define the Settings class (provided context)
public class OpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
}

// 2. Define the Request Model (provided context)
public class OpenAiRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("model")]
    public string Model { get; set; } = "gpt-4o-mini";

    [System.Text.Json.Serialization.JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();

    public class Message
    {
        [System.Text.Json.Serialization.JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}

// 3. Define the Typed Client
public class OpenAiChatClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiSettings _settings;

    public OpenAiChatClient(HttpClient httpClient, IOptions<OpenAiSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;

        // Configure the HttpClient instance specifically for this client
        // Note: IHttpClientFactory ensures these are applied to a fresh handler
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        
        // Set default headers
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<string> GetCompletionAsync(string prompt)
    {
        var requestPayload = new OpenAiRequest
        {
            Messages = new List<OpenAiRequest.Message>
            {
                new OpenAiRequest.Message { Role = "user", Content = prompt }
            }
        };

        // Serialize the payload
        var content = new StringContent(
            JsonSerializer.Serialize(requestPayload), 
            System.Text.Encoding.UTF8, 
            "application/json");

        // Send the request
        var response = await _httpClient.PostAsync("chat/completions", content);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }
}

// 4. Registration (Example in Program.cs or Startup.cs)
public static class ServiceRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configure Options
        services.Configure<OpenAiSettings>(options =>
        {
            options.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "sk-...";
            options.BaseUrl = "https://api.openai.com/v1/";
        });

        // Register the Typed Client
        services.AddHttpClient<OpenAiChatClient>();
    }
}

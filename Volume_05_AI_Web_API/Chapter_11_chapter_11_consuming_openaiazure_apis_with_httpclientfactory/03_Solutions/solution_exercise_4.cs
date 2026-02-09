
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System.Net.Http.Headers;
using System.Text.Json;

// 1. Refactored Builder
public class OpenAiRequestBuilder
{
    private string _systemPrompt = "You are a helpful assistant.";
    private float _temperature = 0.7f;
    private string _userPrompt = string.Empty;
    private readonly Func<Task<string>> _tokenProvider;

    public OpenAiRequestBuilder(Func<Task<string>> tokenProvider)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    public OpenAiRequestBuilder WithSystemPrompt(string prompt)
    {
        _systemPrompt = prompt;
        return this;
    }

    public OpenAiRequestBuilder WithTemperature(float temp)
    {
        _temperature = temp;
        return this;
    }

    public OpenAiRequestBuilder WithUserPrompt(string prompt)
    {
        _userPrompt = prompt;
        return this;
    }

    // 2. Asynchronous Build method
    public async Task<HttpRequestMessage> BuildAsync()
    {
        string accessToken;
        try
        {
            // Fetch token asynchronously
            accessToken = await _tokenProvider();
        }
        catch (Exception ex)
        {
            // 5. Edge Case: Specific exception on token failure
            throw new AuthenticationException("Failed to acquire Azure AD access token.", ex);
        }

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new AuthenticationException("Token provider returned null or empty.");

        // Construct the payload
        var requestPayload = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = _systemPrompt },
                new { role = "user", content = _userPrompt }
            },
            temperature = _temperature
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestPayload), System.Text.Encoding.UTF8, "application/json")
        };

        // 3. Dynamic Header Injection
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        // Azure AD often uses api-key header as well, depending on deployment
        request.Headers.Add("api-key", accessToken); 

        return request;
    }
}

// 4. Refactored Service
public class AzureOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureOpenAiService> _logger;

    public AzureOpenAiService(HttpClient httpClient, ILogger<AzureOpenAiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string userPrompt)
    {
        // Simulate Azure AD token retrieval
        async Task<string> fetchTokenAsync() 
        {
            // In reality, this calls MSAL or Azure Identity library
            await Task.Delay(100); 
            return "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."; 
        }

        var builder = new OpenAiRequestBuilder(fetchTokenAsync)
            .WithUserPrompt(userPrompt)
            .WithTemperature(0.5f);

        // Build the request (this might throw AuthenticationException)
        var request = await builder.BuildAsync();

        // 3. Streaming Integration: SendAsync with ResponseHeadersRead
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        
        response.EnsureSuccessStatusCode();
        
        // Read the full response (or stream it if needed)
        return await response.Content.ReadAsStringAsync();
    }
}

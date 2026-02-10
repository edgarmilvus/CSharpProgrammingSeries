
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
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// 1. Strongly typed response model
public record InferenceResult(
    [property: JsonPropertyName("sentiment")] string Sentiment,
    [property: JsonPropertyName("confidence")] float Confidence
);

public class InferenceClient
{
    private readonly HttpClient _httpClient;

    public InferenceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // BaseAddress is typically configured in DI, but shown here for context
        _httpClient.BaseAddress = new Uri("http://localhost:5000");
    }

    public async Task<InferenceResult> PredictAsync(string text)
    {
        // 2. Create the payload
        var payload = new { text = text };
        
        // 3. Send POST request and deserialize automatically
        // Using PostAsJsonAsync for convenience, handles serialization internally
        var response = await _httpClient.PostAsJsonAsync("/predict", payload);
        
        // 4. Ensure success and deserialize
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<InferenceResult>();
        
        return result ?? throw new InvalidOperationException("Received empty response");
    }
}

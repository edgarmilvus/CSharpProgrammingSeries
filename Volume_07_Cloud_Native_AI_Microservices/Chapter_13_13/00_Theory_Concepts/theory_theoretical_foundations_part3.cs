
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// A tool definition representing an external API call.
public interface ITool
{
    string Name { get; }
    Task<string> ExecuteAsync(string parameters);
}

// Example: A tool to fetch weather data.
public class WeatherTool : ITool
{
    private readonly HttpClient _httpClient;

    public WeatherTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Name => "GetWeather";

    public async Task<string> ExecuteAsync(string parameters)
    {
        // Deserialize parameters (e.g., {"city": "Seattle"})
        var request = JsonSerializer.Deserialize<WeatherRequest>(parameters);
        
        // Call the external microservice
        var response = await _httpClient.GetAsync($"https://api.weather.com/v1/{request.City}");
        
        return await response.Content.ReadAsStringAsync();
    }
}

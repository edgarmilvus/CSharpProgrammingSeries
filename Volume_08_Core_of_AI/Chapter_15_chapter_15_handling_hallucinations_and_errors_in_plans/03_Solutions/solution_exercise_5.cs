
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;

// --- Circuit Breaker Logic ---
public class CircuitBreaker
{
    private int _failureCount = 0;
    private readonly int _threshold;
    private readonly TimeSpan _cooldown;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private bool _isOpen = false;

    public CircuitBreaker(int threshold, TimeSpan cooldown)
    {
        _threshold = threshold;
        _cooldown = cooldown;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_isOpen)
        {
            if (DateTime.UtcNow - _lastFailureTime > _cooldown)
            {
                _isOpen = false; // Half-Open
                _failureCount = 0;
                Console.WriteLine("Circuit Breaker: Half-Open. Testing connection...");
            }
            else
            {
                throw new Exception("Circuit Breaker: Open. Request rejected.");
            }
        }

        try
        {
            var result = await action();
            // Success resets the breaker
            _failureCount = 0;
            _isOpen = false;
            return result;
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            if (_failureCount >= _threshold)
            {
                _isOpen = true;
                Console.WriteLine($"Circuit Breaker: OPENED due to {ex.Message}");
            }
            throw;
        }
    }
}

// --- Plugins ---
public class FinancialDataPlugin
{
    private readonly CircuitBreaker _circuitBreaker = new(3, TimeSpan.FromSeconds(10));
    private readonly Random _random = new();

    [KernelFunction("get_revenue_data")]
    public async Task<string> GetRevenueDataAsync()
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            await Task.Delay(200); // Simulate API call
            
            // Simulate failure
            if (_random.Next(100) < 30) 
                throw new HttpRequestException("API Unavailable");

            return JsonSerializer.Serialize(new { Revenue = 100000, Expenses = 40000 });
        });
    }
}

public class ReportGeneratorPlugin
{
    [KernelFunction("generate_summary")]
    public async Task<string> GenerateSummaryAsync(Kernel kernel, string data)
    {
        var settings = new OpenAIPromptExecutionSettings 
        { 
            ResponseFormat = "json_object", 
            Temperature = 0.1 
        };

        string prompt = $"Analyze this financial data: {data}. Return JSON: {{ summary: string, sentiment: string }}";
        
        // Simulate LLM call
        var result = await kernel.InvokePromptAsync<string>(prompt, new KernelArguments(settings));
        
        // Validate Schema (Simple check)
        try
        {
            JsonDocument.Parse(result);
            return result;
        }
        catch
        {
            throw new ValidationException("Generated report is not valid JSON.");
        }
    }
}

// --- Orchestrator ---
public class FinancialReportAgent
{
    private readonly Kernel _kernel;
    private readonly FinancialDataPlugin _dataPlugin;
    private readonly ReportGeneratorPlugin _reportPlugin;

    public FinancialReportAgent(Kernel kernel)
    {
        _kernel = kernel;
        _dataPlugin = new FinancialDataPlugin();
        _reportPlugin = new ReportGeneratorPlugin();
        
        _kernel.Plugins.AddFromObject(_dataPlugin);
        _kernel.Plugins.AddFromObject(_reportPlugin);
    }

    public async Task<string> GenerateReportAsync()
    {
        int maxRetries = 2;
        
        // 1. Data Fetching with Retry & Validation
        string rawData = string.Empty;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                rawData = await _kernel.InvokeAsync<string>("FinancialDataPlugin", "get_revenue_data");
                
                // Validate Data Logic
                using var doc = JsonDocument.Parse(rawData);
                var revenue = doc.RootElement.GetProperty("Revenue").GetInt32();
                if (revenue < 0) throw new ValidationException("Revenue cannot be negative.");
                
                break; // Success
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Data Validation Failed: {ex.Message}");
                if (i == maxRetries - 1) throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Data Fetch Failed: {ex.Message}");
                // Circuit Breaker logic handles the internal counting, 
                // but we catch here to allow the retry loop to continue if possible.
                if (i == maxRetries - 1) throw;
            }
        }

        // 2. Report Generation with Fallback
        try
        {
            var reportJson = await _kernel.InvokeAsync<string>("ReportGeneratorPlugin", "generate_summary", 
                new KernelArguments { ["data"] = rawData });
            
            return reportJson;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Report Generation Failed: {ex.Message}. Using Template Fallback.");
            return FallbackTemplateGenerator(rawData);
        }
    }

    private string FallbackTemplateGenerator(string data)
    {
        // Simple deterministic fallback
        return JsonSerializer.Serialize(new 
        { 
            summary = "Financial report generated via fallback template.", 
            sentiment = "Neutral" 
        });
    }
}

// --- Main Execution ---
public class Program
{
    public static async Task Main()
    {
        var kernel = Kernel.CreateBuilder().Build();
        var agent = new FinancialReportAgent(kernel);

        try
        {
            // Simulate multiple runs to trigger Circuit Breaker
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"\n--- Run {i + 1} ---");
                string report = await agent.GenerateReportAsync();
                Console.WriteLine($"Report: {report}");
                await Task.Delay(1000);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Workflow Critical Failure: {e.Message}");
        }
    }
}

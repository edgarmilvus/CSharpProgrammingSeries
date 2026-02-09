
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System.ComponentModel;
using System.Text.Json;

// Custom Exception
public class SqlException : Exception
{
    public SqlException(string message) : base(message) { }
}

// 1. Database Plugin with simulated failure
public class DatabasePlugin
{
    private readonly Random _random = new();

    [KernelFunction("query_sales_data")]
    [Description("Queries the sales database.")]
    public async Task<string> QuerySalesDataAsync(Kernel kernel, string query, string complexityLevel = "Advanced")
    {
        await Task.Delay(100); // Simulate network latency

        int chance = _random.Next(100);

        if (chance < 40)
        {
            throw new SqlException("Connection timeout: The server is taking too long to respond.");
        }
        else if (chance < 70)
        {
            return "{}"; // Empty JSON
        }
        else
        {
            // Return valid data, but format depends on complexity (simulated)
            return JsonSerializer.Serialize(new { 
                sales = 1000, 
                region = "North", 
                complexity = complexityLevel 
            });
        }
    }
}

public class FallbackPlugin
{
    [KernelFunction("summarize_offline_data")]
    [Description("Summarizes offline data when DB is unavailable.")]
    public string SummarizeOfflineData()
    {
        return "Offline Data Summary: Total Sales (Estimate): $50,000";
    }
}

// 2. Workflow Orchestrator
public class DataAnalysisAgent
{
    private readonly Kernel _kernel;
    private readonly DatabasePlugin _dbPlugin;
    private readonly FallbackPlugin _fallbackPlugin;

    public DataAnalysisAgent(Kernel kernel)
    {
        _kernel = kernel;
        _dbPlugin = new DatabasePlugin();
        _fallbackPlugin = new FallbackPlugin();
        
        // Import plugins
        _kernel.Plugins.AddFromObject(_dbPlugin);
        _kernel.Plugins.AddFromObject(_fallbackPlugin);
    }

    public async Task<string> AnalyzeSalesAsync()
    {
        // 3. Configure Retry Policy
        var retryOptions = new FunctionRetryOptions
        {
            MaxRetryAttempts = 3,
            OnRetry = (attempt, exception) =>
            {
                Console.WriteLine($"Retry attempt {attempt} due to: {exception.Message}");
                
                // Interactive Challenge: Self-Correction Logic
                // We cannot modify arguments directly in the standard OnRetry callback of FunctionRetryOptions 
                // effectively for the *current* execution without a wrapper or middleware. 
                // However, for this exercise, we will implement a manual retry loop inside the function 
                // to demonstrate argument modification, as standard policies handle the retry but not argument mutation easily.
                return Task.CompletedTask;
            }
        };

        // Note: Standard FunctionRetryOptions handles the loop, but modifying arguments (complexityLevel) 
        // requires a wrapper approach or custom middleware. We will use a manual loop here to satisfy 
        // the "Interactive Challenge" requirement of modifying arguments.
        
        string complexity = "Advanced";
        int attempts = 0;

        while (attempts <= 3)
        {
            try
            {
                attempts++;
                Console.WriteLine($"Executing query with complexity: {complexity}");
                
                // Invoke the function
                var result = await _kernel.InvokeAsync<string>("DatabasePlugin", "query_sales_data", 
                    new KernelArguments { ["query"] = "SELECT * FROM Sales", ["complexityLevel"] = complexity });

                if (string.IsNullOrWhiteSpace(result) || result == "{}")
                {
                    throw new SqlException("Empty result set returned.");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {attempts} failed: {ex.Message}");

                if (attempts >= 3)
                {
                    Console.WriteLine("Primary strategy failed. Switching to fallback.");
                    return await _kernel.InvokeAsync<string>("FallbackPlugin", "summarize_offline_data");
                }

                // Interactive Challenge: Modify arguments for next retry
                if (complexity == "Advanced")
                {
                    complexity = "Basic"; // Self-correction: simplify query
                    Console.WriteLine("Strategy changed: Reducing complexity to 'Basic'.");
                }
            }
        }

        return "Analysis failed.";
    }
}

// Usage
public class Program
{
    public static async Task Main()
    {
        var kernel = Kernel.CreateBuilder().Build();
        var agent = new DataAnalysisAgent(kernel);
        string result = await agent.AnalyzeSalesAsync();
        Console.WriteLine($"Final Result: {result}");
    }
}

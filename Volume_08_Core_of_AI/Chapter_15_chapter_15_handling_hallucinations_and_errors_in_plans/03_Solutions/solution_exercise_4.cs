
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.SemanticKernel;
using System.ComponentModel;

// 1. Interface and Implementations
public interface IImageAnalyzer
{
    Task<string> AnalyzeAsync(Stream imageStream);
}

public class LlmImageAnalyzer : IImageAnalyzer
{
    private readonly Kernel _kernel;
    public LlmImageAnalyzer(Kernel kernel) { _kernel = kernel; }

    public async Task<string> AnalyzeAsync(Stream imageStream)
    {
        // Simulate LLM processing time and potential failure
        await Task.Delay(2000); 
        if (new Random().Next(100) < 50) throw new ModelUnavailableException("Vision model timed out.");
        return "LLM Analysis: A scenic mountain view.";
    }
}

public class LocalImageAnalyzer : IImageAnalyzer
{
    public async Task<string> AnalyzeAsync(Stream imageStream)
    {
        await Task.Delay(500); // Faster local processing
        return "Local Analysis: Mountain landscape detected.";
    }
}

public class MetadataImageAnalyzer : IImageAnalyzer
{
    public async Task<string> AnalyzeAsync(Stream imageStream)
    {
        await Task.Delay(100); // Fastest metadata read
        return "Metadata Analysis: GPS: 45.5N, 122.6W | Camera: DSLR";
    }
}

public class ModelUnavailableException : Exception
{
    public ModelUnavailableException(string message) : base(message) { }
}

// 2. Fallback Orchestrator
public class FallbackOrchestrator
{
    private readonly List<IImageAnalyzer> _analyzers;

    public FallbackOrchestrator(IEnumerable<IImageAnalyzer> analyzers)
    {
        _analyzers = analyzers.ToList();
    }

    // Interactive Challenge: Parallel Fallbacks
    public async Task<string> AnalyzeWithFallbackAsync(Stream imageStream)
    {
        // 1. Attempt Primary (LLM) with Timeout
        var primaryAnalyzer = _analyzers.First();
        var primaryTask = primaryAnalyzer.AnalyzeAsync(imageStream);
        
        // Timeout of 5 seconds
        var timeoutTask = Task.Delay(5000);
        var completedTask = await Task.WhenAny(primaryTask, timeoutTask);

        if (completedTask == primaryTask && !primaryTask.IsFaulted)
        {
            var result = await primaryTask;
            if (!result.Contains("I don't know", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Success: Primary LLM Analyzer used.");
                return result;
            }
        }

        Console.WriteLine("Primary failed or timed out. Switching to parallel fallbacks...");

        // 2. Parallel Fallbacks (Local + Metadata)
        var fallbackAnalyzers = _analyzers.Skip(1).ToList();
        var fallbackTasks = fallbackAnalyzers.Select(a => a.AnalyzeAsync(imageStream)).ToList();

        // Wait for the first successful result
        while (fallbackTasks.Count > 0)
        {
            var finishedTask = await Task.WhenAny(fallbackTasks);
            fallbackTasks.Remove(finishedTask);

            try
            {
                var result = await finishedTask;
                // Check for hallucination (e.g., "I don't know" or empty)
                if (!string.IsNullOrEmpty(result) && !result.Contains("I don't know"))
                {
                    Console.WriteLine($"Success: Fallback analyzer used.");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fallback analyzer failed: {ex.Message}");
            }
        }

        return "All analysis attempts failed.";
    }
}

// Usage
public class Program
{
    public static async Task Main()
    {
        var kernel = Kernel.CreateBuilder().Build();
        
        var analyzers = new List<IImageAnalyzer>
        {
            new LlmImageAnalyzer(kernel),
            new LocalImageAnalyzer(),
            new MetadataImageAnalyzer()
        };

        var orchestrator = new FallbackOrchestrator(analyzers);
        
        using var stream = new MemoryStream(); // Dummy stream
        string result = await orchestrator.AnalyzeWithFallbackAsync(stream);
        
        Console.WriteLine($"Final Result: {result}");
    }
}

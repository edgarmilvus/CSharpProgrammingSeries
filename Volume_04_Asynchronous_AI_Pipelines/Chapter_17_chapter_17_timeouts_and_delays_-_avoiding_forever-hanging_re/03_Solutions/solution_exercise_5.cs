
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public enum ResponseSource { Primary, Cache, Local }

public class LlmResult
{
    public ResponseSource Source { get; set; }
    public string Content { get; set; }
    public double LatencyMs { get; set; }
    public bool WasFallback { get; set; }
}

public class ResilientLLMOrchestrator
{
    // Simulated dependencies
    private readonly ILLMProvider _primaryLlm;
    private readonly ILLMProvider _cachedLlm;
    private readonly ILLMProvider _fastLocalLlm;

    public ResilientLLMOrchestrator(ILLMProvider primary, ILLMProvider cache, ILLMProvider local)
    {
        _primaryLlm = primary;
        _cachedLlm = cache;
        _fastLocalLlm = local;
    }

    public async Task<LlmResult> GetResponseAsync(string prompt)
    {
        var attempts = new List<Exception>();
        var sw = Stopwatch.StartNew();

        // 1. Attempt Primary LLM
        try
        {
            var result = await _primaryLlm.GenerateAsync(prompt, TimeSpan.FromSeconds(5));
            sw.Stop();
            return new LlmResult 
            { 
                Source = ResponseSource.Primary, 
                Content = result, 
                LatencyMs = sw.ElapsedMilliseconds,
                WasFallback = false
            };
        }
        catch (Exception ex)
        {
            attempts.Add(ex);
            // Log warning: Primary failed, attempting fallback...
        }

        // 2. Attempt Cached LLM
        try
        {
            var result = await _cachedLlm.GenerateAsync(prompt, TimeSpan.FromSeconds(1));
            sw.Stop();
            return new LlmResult 
            { 
                Source = ResponseSource.Cache, 
                Content = result, 
                LatencyMs = sw.ElapsedMilliseconds,
                WasFallback = true
            };
        }
        catch (Exception ex)
        {
            attempts.Add(ex);
            // Log warning: Cache failed, attempting local...
        }

        // 3. Attempt Fast Local LLM
        try
        {
            var result = await _fastLocalLlm.GenerateAsync(prompt, TimeSpan.FromSeconds(3));
            sw.Stop();
            return new LlmResult 
            { 
                Source = ResponseSource.Local, 
                Content = result, 
                LatencyMs = sw.ElapsedMilliseconds,
                WasFallback = true
            };
        }
        catch (Exception ex)
        {
            attempts.Add(ex);
        }

        // 4. All failed
        throw new AggregateException("All LLM fallback levels failed.", attempts);
    }
}

// Mock Interface for demonstration
public interface ILLMProvider
{
    Task<string> GenerateAsync(string prompt, TimeSpan timeout);
}


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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Runtime.CompilerServices;

// 1. The Streaming Service
public class StreamingInferenceService : IDisposable
{
    private readonly InferenceSession _session;

    public StreamingInferenceService(string modelPath)
    {
        // Simplified session creation for context
        _session = new InferenceSession(modelPath); 
    }

    // IAsyncEnumerable allows yielding tokens as they are generated
    public async IAsyncEnumerable<string> GenerateCodeAsync(
        string prompt, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // NOTE: In a real scenario, this involves complex tokenizer logic 
        // and an iterative loop (e.g., while not end of sequence).
        // Here we simulate the streaming behavior.
        
        // Mocking the token generation loop
        var mockTokens = new[] { "public", " ", "class", " ", "Program", " ", "{", " ", "}" };

        foreach (var token in mockTokens)
        {
            // Check for cancellation before yielding
            cancellationToken.ThrowIfCancellationRequested();

            // Simulate network/compute delay
            await Task.Delay(100, cancellationToken); 
            
            yield return token;
        }
    }

    public void Dispose() => _session?.Dispose();
}

// 2. The View Model (e.g., for WPF/MAUI using CommunityToolkit.Mvvm)
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableObject
{
    private readonly StreamingInferenceService _inferenceService;
    
    [ObservableProperty]
    private string _generatedCode = "";

    [ObservableProperty]
    private bool _isGenerating;

    // SynchronizationContext to marshal back to UI thread
    private readonly SynchronizationContext? _uiContext;

    public MainViewModel(StreamingInferenceService inferenceService)
    {
        _inferenceService = inferenceService;
        // Capture the UI context (Main thread) for WPF/WinUI
        _uiContext = SynchronizationContext.Current; 
    }

    [RelayCommand]
    private async Task StartGenerationAsync()
    {
        if (IsGenerating) return;

        IsGenerating = true;
        GeneratedCode = string.Empty;
        
        var cts = new CancellationTokenSource();

        try
        {
            // 3. The await foreach loop (Non-blocking)
            await foreach (var token in _inferenceService.GenerateCodeAsync("Write code", cts.Token))
            {
                // 4. Update UI on the UI Thread
                // We cannot update UI directly from the background thread where IAsyncEnumerable yields
                if (_uiContext != null)
                {
                    _uiContext.Post(_ => 
                    {
                        GeneratedCode += token; 
                    }, null);
                }
                else
                {
                    // Fallback for unit tests or specific environments
                    GeneratedCode += token;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully
            GeneratedCode += "\n[Generation Cancelled]";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private void StopGeneration()
    {
        // Logic to trigger cancellation via CancellationTokenSource
        // Implementation depends on where the CTS is stored (field vs local)
    }
}

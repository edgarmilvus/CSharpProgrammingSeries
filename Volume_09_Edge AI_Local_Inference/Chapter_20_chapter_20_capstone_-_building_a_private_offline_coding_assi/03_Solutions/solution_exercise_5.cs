
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// 1. Architecture: Services
public class FileWatcherService : IDisposable
{
    private readonly FileSystemWatcher _watcher;

    public FileWatcherService(string path, Action<string> onFileChanged)
    {
        _watcher = new FileSystemWatcher(path, "*.cs");
        _watcher.Changed += (s, e) => onFileChanged(e.FullPath);
        _watcher.EnableRaisingEvents = true;
    }

    public void Dispose() => _watcher.Dispose();
}

public class InferenceService : IDisposable
{
    private readonly InferenceSession _session;
    public InferenceService(string modelPath) => _session = new InferenceSession(modelPath);
    
    // Simplified generation for the capstone
    public async IAsyncEnumerable<string> GenerateAsync(string prompt)
    {
        // In reality: Tokenize -> Run Session -> Detokenize loop
        yield return "Based on your code, here is the answer: ";
        await Task.Delay(100);
        yield return "public void Method() { ... }";
    }

    public void Dispose() => _session?.Dispose();
}

// 2. Main ViewModel coordinating the workflow
public partial class MainViewModel : ObservableObject, IDisposable
{
    // Dependencies
    private readonly VectorStoreService _vectorStore;
    private readonly InferenceService _inferenceService;
    private readonly FileWatcherService _fileWatcher;
    
    // State
    [ObservableProperty] private string _chatLog = "";
    [ObservableProperty] private string _contextLog = "";
    [ObservableProperty] private string _modelStatus = "Ready";
    [ObservableProperty] private bool _isBusy;

    public MainViewModel(VectorStoreService vectorStore, InferenceService inferenceService)
    {
        _vectorStore = vectorStore;
        _inferenceService = inferenceService;
        
        // Initialize File Watcher to monitor ./src
        _fileWatcher = new FileWatcherService("./src", async (path) => 
        {
            // Hot-reload logic
            ModelStatus = $"Indexing change: {Path.GetFileName(path)}";
            await _vectorStore.BuildIndexAsync("./src"); // Rebuild or update specific doc
            ModelStatus = "Index Updated";
        });
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        ModelStatus = "Building Index...";
        
        // Scan ./src folder on startup
        await _vectorStore.BuildIndexAsync("./src");
        
        IsBusy = false;
        ModelStatus = "Ready (RAG Active)";
    }

    [RelayCommand]
    public async Task AskQuestionAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return;

        IsBusy = true;
        ChatLog += $"\n[User]: {question}";

        try
        {
            // 1. RAG: Retrieve relevant context
            var relevantDocs = _vectorStore.Search(question).ToList();
            
            // 2. Update Context Panel
            ContextLog = string.Join("\n---\n", relevantDocs.Select(d => d.FilePath));
            
            // 3. Construct Prompt with Context
            var contextString = string.Join("\n", relevantDocs.Select(d => d.CodeContent));
            var finalPrompt = $"Context:\n{contextString}\n\nQuestion: {question}\nAnswer:";

            // 4. Stream Response
            ChatLog += "\n[Assistant]: ";
            await foreach (var token in _inferenceService.GenerateAsync(finalPrompt))
            {
                ChatLog += token;
            }
        }
        catch (Exception ex)
        {
            ChatLog += $"\n[Error]: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // 3. Resource Management
    public void Dispose()
    {
        _inferenceService.Dispose();
        _fileWatcher.Dispose();
    }
}

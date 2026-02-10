
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

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Assuming standard logging is available

// Interface defining the contract for an AI model
public interface IAIModel
{
    Task<InferenceResponse> PredictAsync(InferenceRequest request);
    Task InitializeAsync();
    bool IsReady { get; }
}

public class ModelManager
{
    private IAIModel _currentModel;
    private IAIModel _stagingModel;
    
    // The atomic delegate pointer
    private Func<InferenceRequest, Task<InferenceResponse>> _activePredictionDelegate;
    
    // Synchronization primitive to ensure only one swap happens at a time
    private readonly SemaphoreSlim _swapLock = new SemaphoreSlim(1, 1);
    
    private readonly ILogger<ModelManager> _logger;

    public ModelManager(IAIModel initialModel, ILogger<ModelManager> logger)
    {
        _logger = logger;
        _currentModel = initialModel;
        // Initialize the delegate to point to the current model's PredictAsync
        _activePredictionDelegate = req => _currentModel.PredictAsync(req);
    }

    // Expose the delegate for the InferenceService to use
    public Func<InferenceRequest, Task<InferenceResponse>> ActivePredictor => _activePredictionDelegate;

    // 1. Load Staging Model
    public async Task LoadStagingModelAsync(string modelPath)
    {
        // In a real app, we would instantiate the specific model implementation here
        // For simulation, we assume a factory or dependency injection creates the IAIModel
        // _stagingModel = _modelFactory.Create(modelPath);
        
        _logger.LogInformation($"Starting background load of model from {modelPath}");
        
        try
        {
            // Simulate loading delay
            await Task.Delay(2000); 
            
            // Simulate the model initialization
            await _stagingModel.InitializeAsync();
            
            _logger.LogInformation("Staging model loaded and initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize staging model.");
            _stagingModel = null; // Clear failed model
            throw;
        }
    }

    // 2. Swap Models
    public async Task SwapModelsAsync()
    {
        if (!await _swapLock.WaitAsync(TimeSpan.FromSeconds(5)))
        {
            throw new InvalidOperationException("Swap operation already in progress.");
        }

        try
        {
            if (_stagingModel == null || !_stagingModel.IsReady)
            {
                throw new InvalidOperationException("No valid staging model available to swap.");
            }

            // Prepare the new delegate
            var newDelegate = _stagingModel.PredictAsync;

            // SOLUTION: Atomic update of the delegate.
            // Interlocked.Exchange is thread-safe and ensures that any thread reading 
            // ActivePredictor gets either the old or the new delegate, never a null or partial state.
            var oldDelegate = Interlocked.Exchange(ref _activePredictionDelegate, newDelegate);

            // Update references
            _currentModel = _stagingModel;
            _stagingModel = null; // Clear staging

            _logger.LogInformation("Model swap completed atomically.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during model swap.");
            // Failure handling: The _currentModel and _activePredictionDelegate remain unchanged
            // effectively keeping the system on the old model.
            throw;
        }
        finally
        {
            _swapLock.Release();
        }
    }
}

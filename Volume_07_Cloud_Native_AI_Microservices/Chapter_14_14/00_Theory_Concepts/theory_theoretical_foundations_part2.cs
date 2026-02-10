
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

using Microsoft.ML.OnnxRuntime; // Example namespace for ONNX Runtime

// Interface defined in a previous chapter regarding dependency injection
public interface IInferenceEngine
{
    Task<string> GenerateAsync(string prompt);
}

// Implementation targeting a specific GPU partition (MIG slice)
public class OnnxInferenceEngine : IInferenceEngine, IDisposable
{
    private readonly InferenceSession _session;
    private readonly int _gpuDeviceId;

    public OnnxInferenceEngine(string modelPath, int gpuDeviceId)
    {
        _gpuDeviceId = gpuDeviceId;
        // Configuring session options to bind to a specific GPU instance
        var options = new SessionOptions();
        options.AppendExecutionProvider_CUDA(gpuDeviceId);
        
        // Loading the model into VRAM (expensive operation)
        _session = new InferenceSession(modelPath, options);
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        // Execution logic here
        return await Task.Run(() => "Generated response");
    }

    public void Dispose()
    {
        // Critical: Explicitly release GPU memory to avoid fragmentation
        _session.Dispose();
    }
}

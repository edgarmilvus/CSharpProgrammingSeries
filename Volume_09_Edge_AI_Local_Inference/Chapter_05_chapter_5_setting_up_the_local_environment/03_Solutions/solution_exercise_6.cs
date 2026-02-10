
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

using System;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalAI.Inference
{
    public static class DynamicShapeConfigurator
    {
        public static SessionOptions ConfigureDynamicShapes()
        {
            var options = new SessionOptions();

            // 1. Set Execution Mode
            options.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;

            // 2. Enable Memory Pattern Optimization
            options.EnableMemoryPattern = true;

            // 3. Set Graph Optimization Level
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

            // Note: To truly support dynamic shapes, the ONNX model itself must have 
            // dynamic axes defined (e.g., input shape [1, 'seq_len']).
            // The SessionOptions configures *how* to run, not necessarily *what* shapes are allowed 
            // (that is usually baked into the model or handled by the runtime automatically if dynamic axes exist).

            return options;
        }

        public static OrtValue CreateDynamicTensor(ReadOnlySpan<int> tokenIds)
        {
            // We want a shape of [1, currentTokenCount]
            // Batch size = 1, Sequence Length = tokenIds.Length
            long[] shape = new long[] { 1, tokenIds.Length };

            // Create the OrtValue directly from the Span
            // This avoids copying data to intermediate arrays.
            // OrtValue owns the underlying native memory.
            var inputTensor = new OrtValue(OrtMemoryInfo.DefaultInstance, tokenIds, shape);

            return inputTensor;
        }
    }
}

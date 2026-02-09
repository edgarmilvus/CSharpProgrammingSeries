
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

// 1. Define options to enable quantization
var options = new SessionOptions();

// 2. Enable Dynamic Quantization specifically for CPU execution
// This tells the runtime to quantize weights from FP32 to INT8 
// on-the-fly, just before execution.
options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

// 3. Configure the execution provider (CPU)
// Note: For INT4, we often rely on specific kernels or 
// quantization tools (like Olive) to prepare the model beforehand.
options.AppendExecutionProvider_CPU();

// 4. Load the model
// The runtime parses the ONNX graph. If the model was pre-quantized 
// (e.g., via ONNX Runtime Tools), it detects the INT8/INT4 tensors.
// If not, dynamic quantization applies the mapping now.
using var session = new InferenceSession("model.onnx", options);

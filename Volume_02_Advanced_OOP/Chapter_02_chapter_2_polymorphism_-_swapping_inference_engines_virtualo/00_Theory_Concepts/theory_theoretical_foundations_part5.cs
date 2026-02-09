
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

# Source File: theory_theoretical_foundations_part5.cs
# Description: Theoretical Foundations
# ==========================================

// Abstracting Tensor Operations
public interface ITensorOperation
{
    // Returns a generic object representing the tensor result
    object Multiply(object matrixA, object matrixB);
}

// Implementation for a high-performance GPU library (e.g., a wrapper around a C++ library)
public class CudaTensorOperation : ITensorOperation
{
    public object Multiply(object matrixA, object matrixB)
    {
        // Unmanaged memory pointers logic here
        Console.WriteLine("Performing high-speed matrix multiplication on GPU...");
        return new { Data = "GPU Result" };
    }
}

// Implementation for a standard .NET library (e.g., Math.NET)
public class CpuTensorOperation : ITensorOperation
{
    public object Multiply(object matrixA, object matrixB)
    {
        // Managed array logic here
        Console.WriteLine("Performing matrix multiplication on CPU...");
        return new { Data = "CPU Result" };
    }
}

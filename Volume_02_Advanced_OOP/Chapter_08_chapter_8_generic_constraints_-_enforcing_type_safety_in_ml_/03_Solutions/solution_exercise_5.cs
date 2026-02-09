
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

namespace MLEngine.GenericConstraints
{
    // 1. Domain-specific tensor markers
    public interface IImageTensor { }
    public interface ITokenTensor { }

    // 2. Concrete Tensor Implementations
    public struct ImageFloatTensor : IImageTensor
    {
        public float[,,] Data;
        public ImageFloatTensor(int w, int h, int c) { Data = new float[c, h, w]; }
    }

    public struct TokenIntTensor : ITokenTensor
    {
        public int[] Data;
        public TokenIntTensor(int len) { Data = new int[len]; }
    }

    // 3. Generic Input Wrapper
    public class ModelInput<T> where T : struct
    {
        public T TensorData { get; set; }
    }

    // 4. Model Runner with Strict Method Constraints
    public class ModelRunner
    {
        public void RunImageModel(ModelInput<IImageTensor> input)
        {
            Console.WriteLine("Running Image Model (CNN)...");
        }

        public void RunTokenModel(ModelInput<ITokenTensor> input)
        {
            Console.WriteLine("Running Token Model (Transformer)...");
        }
    }

    public class Exercise5Runner
    {
        public static void Run()
        {
            var runner = new ModelRunner();

            // Setup Data
            var imgInput = new ModelInput<ImageFloatTensor> { TensorData = new ImageFloatTensor(224, 224, 3) };
            var tokenInput = new ModelInput<TokenIntTensor> { TensorData = new TokenIntTensor(512) };

            // Valid Calls
            // Note: Even though ImageFloatTensor implements IImageTensor, 
            // ModelInput<ImageFloatTensor> is NOT a ModelInput<IImageTensor> 
            // because generics are invariant by default.
            // To make this work, we would typically cast or use covariance if the interface supported it.
            // However, for strict separation, we often instantiate the wrapper with the interface type.
            
            var strictImgInput = new ModelInput<IImageTensor> { TensorData = new ImageFloatTensor(224, 224, 3) };
            var strictTokenInput = new ModelInput<ITokenTensor> { TensorData = new TokenIntTensor(512) };

            runner.RunImageModel(strictImgInput); 
            runner.RunTokenModel(strictTokenInput);

            // INVALID COMPILATION EXAMPLES:
            
            // 1. Passing Token wrapper to Image method
            // runner.RunImageModel(strictTokenInput); 
            // Error: Cannot convert ModelInput<ITokenTensor> to ModelInput<IImageTensor>
        }
    }
}

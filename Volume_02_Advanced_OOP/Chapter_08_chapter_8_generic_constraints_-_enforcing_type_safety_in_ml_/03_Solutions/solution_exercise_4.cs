
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

using System;
using System.Collections.Generic;

namespace MLEngine.GenericConstraints
{
    // 1. Interface for a transformation step
    public interface IPipelineStep<TIn, TOut>
    {
        TOut Execute(TIn input);
    }

    // 2. Concrete Steps
    public class NormalizeStep : IPipelineStep<double[], double[]>
    {
        public double[] Execute(double[] input)
        {
            double[] output = new double[input.Length];
            for(int i=0; i<input.Length; i++) output[i] = input[i] / 255.0;
            return output;
        }
    }

    public class InferenceStep : IPipelineStep<double[], int>
    {
        public int Execute(double[] input)
        {
            return input[0] > 0.5 ? 1 : 0;
        }
    }

    // 3. Typed Pipeline Builder using Recursive Generics
    public class TypedPipeline<TCurrent>
    {
        private readonly List<object> _steps = new List<object>();

        // Private constructor used by the chaining method
        private TypedPipeline(List<object> steps)
        {
            _steps = steps;
        }

        // Entry point
        public static TypedPipeline<TStart> Create<TStart>() 
        {
            return new TypedPipeline<TStart>(new List<object>());
        }

        // Chaining method with constraints
        // Returns a TypedPipeline<TNext>, enforcing the chain type safety
        public TypedPipeline<TNext> AddStep<TNext>(IPipelineStep<TCurrent, TNext> step)
        {
            _steps.Add(step);
            return new TypedPipeline<TNext>(_steps);
        }

        public void Run(TStart input) // Assuming TStart is accessible or passed in
        {
             // Execution logic would iterate through _steps
             Console.WriteLine("Pipeline ready for execution.");
        }
    }

    public class Exercise4Runner
    {
        public static void Run()
        {
            // 4. Usage demonstrating type safety
            // TCurrent starts as double[]
            var pipeline = TypedPipeline<double[]>.Create()
                .AddStep(new NormalizeStep()) // Returns double[], so TNext is double[]
                .AddStep(new InferenceStep()); // Takes double[], Returns int. Chain valid.

            Console.WriteLine("Pipeline constructed successfully with type safety.");

            // Compilation failure example (Uncomment to test):
            // var badPipeline = TypedPipeline<int[]>.Create()
            //     .AddStep(new NormalizeStep()); 
            // Error: Argument 1: cannot convert from 'NormalizeStep' to 'IPipelineStep<int[], double[]>'
        }
    }
}

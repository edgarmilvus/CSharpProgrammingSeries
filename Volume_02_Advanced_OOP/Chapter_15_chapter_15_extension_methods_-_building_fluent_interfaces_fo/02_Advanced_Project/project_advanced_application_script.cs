
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;

// NAMESPACE: AdvancedOOP.DataStructures
namespace AdvancedOOP.DataStructures
{
    // ---------------------------------------------------------
    // 1. THE CORE DATA STRUCTURE (The "Existing Code")
    // ---------------------------------------------------------
    // We define a Tensor class. In a real AI engine, this would
    // hold massive multi-dimensional arrays. Here, we simulate
    // a 1D tensor (a vector) with a single value for clarity.
    // We DO NOT modify this class directly.
    // ---------------------------------------------------------
    public class Tensor
    {
        public double Value { get; private set; }

        public Tensor(double value)
        {
            this.Value = value;
        }

        // A helper to visualize the state
        public override string ToString()
        {
            return $"[Tensor Value: {Value:F4}]";
        }
    }

    // ---------------------------------------------------------
    // 2. EXTENSION METHODS (The "Fluent Interface")
    // ---------------------------------------------------------
    // These methods extend the Tensor class. They allow us to
    // attach new functionality to the Tensor without changing
    // its source code.
    // ---------------------------------------------------------
    public static class TensorExtensions
    {
        // EXTENSION METHOD 1: Basic Math (Fluent)
        // 'this Tensor t' makes this a method on Tensor.
        // Returns 'Tensor' to allow chaining (Method Chaining).
        public static Tensor Add(this Tensor t, double scalar)
        {
            // We create a NEW instance. This is functional style.
            // In AI, we often want to avoid mutating state directly.
            return new Tensor(t.Value + scalar);
        }

        // EXTENSION METHOD 2: Multiplication
        public static Tensor Multiply(this Tensor t, double scalar)
        {
            return new Tensor(t.Value * scalar);
        }

        // EXTENSION METHOD 3: The "Power User" Tool (Delegates & Lambdas)
        // This method accepts a DELEGATE (an Action) to perform
        // a custom operation. This allows for dynamic behavior injection.
        public static Tensor ApplyCustomOperation(this Tensor t, Action<Tensor> operation)
        {
            // We execute the lambda passed in
            operation(t);
            return t; // Return the modified instance
        }

        // EXTENSION METHOD 4: The "Fluent Pipe" (Lazy Evaluation Simulation)
        // This accepts a LAMBDA EXPRESSION that returns a Tensor.
        // This allows us to build a pipeline of logic that is only
        // executed when we finally call .Execute().
        public static FluentTensorPipe Pipe(this Tensor t, Func<Tensor, Tensor> pipelineStep)
        {
            return new FluentTensorPipe(pipelineStep(t));
        }
    }

    // ---------------------------------------------------------
    // 3. THE FLUENT PIPELINE WRAPPER (Lazy Evaluation)
    // ---------------------------------------------------------
    // This class helps us build complex chains where we might want
    // to defer execution or add conditional logic later.
    // ---------------------------------------------------------
    public class FluentTensorPipe
    {
        private Tensor _current;

        public FluentTensorPipe(Tensor initial)
        {
            _current = initial;
        }

        // Chain the next step
        public FluentTensorPipe Pipe(Func<Tensor, Tensor> nextStep)
        {
            // Apply the lambda to the current state
            _current = nextStep(_current);
            return this; // Return self to keep chaining
        }

        // Finalize and get result
        public Tensor Execute()
        {
            return _current;
        }
    }

    // ---------------------------------------------------------
    // 4. THE APPLICATION SCRIPT
    // ---------------------------------------------------------
    public class NeuralNetworkBuilder
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("--- AI Tensor Processing Simulation ---\n");

            // Initialize our raw data
            Tensor input = new Tensor(5.0);
            Console.WriteLine($"Initial State: {input}");

            // -------------------------------------------------
            // SCENARIO A: Standard Fluent Chaining
            // -------------------------------------------------
            // We want to: Scale input by 2, then add 10.
            // Before: Add(Multiply(input, 2), 10)
            // After: input.Multiply(2).Add(10)
            
            Tensor resultA = input.Multiply(2).Add(10);
            Console.WriteLine($"\nScenario A (Standard Chain): {resultA}");


            // -------------------------------------------------
            // SCENARIO B: Injecting Custom Logic via Delegates
            // -------------------------------------------------
            // We want to perform a specific, one-off operation
            // (e.g., ReLU activation: if negative, set to 0)
            // without writing a new extension method.
            
            // We define the logic as a LAMBDA EXPRESSION
            Action<Tensor> reluLogic = (t) => 
            {
                if (t.Value < 0) 
                {
                    // Since our Tensor is immutable in the extensions above,
                    // we are simulating an in-place mutation here for demonstration.
                    // In a real struct, we'd need 'ref'.
                    // For this script, we'll assume this modifies the object if it were mutable.
                    // To keep it clean, we will just print the logic here.
                    Console.WriteLine($"    [System: ReLU clamped negative value {t.Value} to 0]");
                }
            };

            Console.WriteLine("\nScenario B (Delegate Injection):");
            // We use our extension method that accepts an Action<Tensor>
            // Note: We are chaining the result of ApplyCustomOperation (which returns Tensor)
            // into a standard Add method.
            Tensor resultB = input.Multiply(2).ApplyCustomOperation(reluLogic).Add(10);
            Console.WriteLine($"Final Result B: {resultB}");


            // -------------------------------------------------
            // SCENARIO C: Complex Lazy Pipeline (Lambda Chaining)
            // -------------------------------------------------
            // We build a complex recipe but don't execute it immediately
            // until we call Execute().
            
            Console.WriteLine("\nScenario C (Lazy Pipeline):");
            
            // We start the pipe
            var pipeline = input.Pipe(t => t.Multiply(3))
                               .Pipe(t => t.Add(100))
                               .Pipe(t => {
                                   // Complex logic inside a lambda
                                   double val = t.Value;
                                   if (val > 50) val = val / 2;
                                   return new Tensor(val);
                               });

            // The operations above are defined but not fully chained in the final variable yet.
            // Actually, the design of FluentTensorPipe executes immediately.
            // Let's adjust Scenario C to show a pure Lambda Chain without the wrapper class,
            // as that is the core focus of "Lambda Expressions" in Book 2.

            // RE-DOING SCENARIO C to strictly focus on Lambda Expressions as Chain Links:
            // We will chain methods that take Func<Tensor, Tensor>.
            
            Func<Tensor, Tensor> step1 = (t) => t.Multiply(3);
            Func<Tensor, Tensor> step2 = (t) => t.Add(100);
            Func<Tensor, Tensor> step3 = (t) => t.Multiply(2);

            // Manual chaining using the delegates
            Tensor resultC = step1(input);       // 5 * 3 = 15
            resultC = step2(resultC);            // 15 + 100 = 115
            resultC = step3(resultC);            // 115 * 2 = 230

            Console.WriteLine($"Pipeline Step 1: {step1(input)}");
            Console.WriteLine($"Pipeline Step 2: {step2(step1(input))}");
            Console.WriteLine($"Final Pipeline Result: {resultC}");

            Console.WriteLine("\n--- Simulation Complete ---");
        }
    }
}

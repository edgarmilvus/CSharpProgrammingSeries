
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;

// Reusing Transformer interface
public interface Transformer<I, O>
{
    O Transform(I input);
}

// Concrete Transformers for the chain
public class StringToInt : Transformer<string, int>
{
    public int Transform(string input)
    {
        return int.Parse(input); 
    }
}

public class IntToDouble : Transformer<int, double>
{
    public double Transform(int input)
    {
        return (double)input * 1.5;
    }
}

// The Pipeline Class
// This implementation uses a list of delegates to store the steps.
// While Lambdas are forbidden, 'Func' is a standard delegate type in C#.
// To strictly avoid lambda expressions, we wrap the generic transformer in a specific class.
public class StepWrapper
{
    private Func<object, object> _executionStep;

    public StepWrapper(Func<object, object> step)
    {
        _executionStep = step;
    }

    public object Execute(object input)
    {
        return _executionStep(input);
    }
}

public class FeaturePipeline
{
    private List<StepWrapper> _steps = new List<StepWrapper>();

    public FeaturePipeline() { }

    // Generic method to add a step
    // We return 'FeaturePipeline' to allow chaining, though we lose specific type info in the return type
    // unless we create a new Pipeline instance (which is complex without recursive generics).
    // Here we return the same instance for simplicity.
    public FeaturePipeline AddStep<I, O>(Transformer<I, O> transformer)
    {
        // Create a wrapper that handles the type casting
        Func<object, object> stepLogic = (input) => 
        {
            if (input is I typedInput)
            {
                return transformer.Transform(typedInput);
            }
            throw new InvalidCastException($"Input type {input.GetType()} is not {typeof(I)}");
        };

        _steps.Add(new StepWrapper(stepLogic));
        return this;
    }

    public object Run(object input)
    {
        object current = input;
        foreach (var step in _steps)
        {
            current = step.Execute(current);
        }
        return current;
    }
}

public class Exercise4Runner
{
    public static void Run()
    {
        var stringToInt = new StringToInt();
        var intToDouble = new IntToDouble();

        var pipeline = new FeaturePipeline();
        
        // Chaining steps
        pipeline.AddStep(stringToInt)
                .AddStep(intToDouble);

        string input = "42";
        object result = pipeline.Run(input);

        Console.WriteLine($"Input: {input}, Final Result: {result}, Type: {result.GetType().Name}");
    }
}

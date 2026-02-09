
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;

// 1. Define the Delegate
public delegate bool ValidationRule<T>(T item);

// 2. Validator Class
public class Validator<T>
{
    private readonly List<ValidationRule<T>> _rules = new List<ValidationRule<T>>();

    public void AddRule(ValidationRule<T> rule)
    {
        _rules.Add(rule);
    }

    // 3. Validate Method
    public bool Validate(T item)
    {
        foreach (var rule in _rules)
        {
            if (!rule(item))
            {
                return false; // Fail fast
            }
        }
        return true;
    }
}

public class TensorValidatorExample
{
    public static void Run()
    {
        var tensor = new Tensor("Test", new int[] { 2, 2 }, new double[] { 1, 2, 3, 4 });
        var validator = new Validator<Tensor>();

        // 4. Using Lambda Expressions to define rules inline
        // Rule 1: Data must not be null or empty
        validator.AddRule(t => t.Data != null && t.Data.Length > 0);

        // Rule 2: Shape must not be null
        validator.AddRule(t => t.Shape != null);

        // Rule 3: Data length must match product of shape dimensions
        validator.AddRule(t => 
        {
            int totalElements = 1;
            foreach (int dim in t.Shape) totalElements *= dim;
            return t.Data.Length == totalElements;
        });

        // Execute Validation
        if (validator.Validate(tensor))
        {
            Console.WriteLine("Tensor validation passed. Ready for serialization.");
            // Proceed to Save...
        }
        else
        {
            Console.WriteLine("Tensor validation failed. Aborting save.");
        }

        // Test with invalid data
        var badTensor = new Tensor("Bad", new int[] { 2, 2 }, new double[] { 1 }); // 1 element vs 4 required
        if (!validator.Validate(badTensor))
        {
            Console.WriteLine($"Validation correctly rejected bad tensor.");
        }
    }
}

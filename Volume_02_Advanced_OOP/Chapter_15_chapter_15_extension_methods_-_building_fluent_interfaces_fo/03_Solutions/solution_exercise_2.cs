
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;

public class LazyChain<TInput, TOutput>
{
    private readonly Func<TInput, TOutput> _transformation;

    // Constructor initializing with an identity function
    public LazyChain(Func<TInput, TOutput> initialTransformation = null)
    {
        // If null, create an identity function that casts input to output
        _transformation = initialTransformation ?? (input => (TOutput)(object)input);
    }

    // Private constructor for internal chaining
    private LazyChain(Func<TInput, TOutput> transformation)
    {
        _transformation = transformation;
    }

    // The fluent method that composes the functions
    public LazyChain<TInput, TNewOutput> Then<TNewOutput>(Func<TOutput, TNewOutput> nextTransformation)
    {
        // Compose: nextTransformation(_transformation(input))
        // This creates a new lambda that wraps the previous logic
        Func<TInput, TNewOutput> composedFunc = input => nextTransformation(_transformation(input));
        
        // Return a new chain instance with the composed function
        return new LazyChain<TInput, TNewOutput>(composedFunc);
    }

    // Execute the chain
    public TOutput Run(TInput input)
    {
        return _transformation(input);
    }
}

public class Program
{
    public static void Main()
    {
        // Start the chain with a double input
        var chain = new LazyChain<double, double>();

        // Apply transformations
        // No calculation happens here yet, just function composition
        var result = chain
            .Then(x => x * 2)   // Step 1
            .Then(x => x + 10)  // Step 2
            .Then(x => x * x)   // Step 3
            .Run(5.0);          // Execution happens here

        // Calculation: 5 * 2 = 10. 10 + 10 = 20. 20 * 20 = 400.
        Console.WriteLine($"Input: 5.0, Result: {result}");
        
        if (result == 400.0)
            Console.WriteLine("Chain execution successful.");
        else
            Console.WriteLine("Chain execution failed.");
    }
}

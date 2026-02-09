
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections;

namespace AdvancedOOP.BoxingBasics
{
    public class LoggingSystem
    {
        public static void Main(string[] args)
        {
            // 1. A list designed to hold any object (System.Object).
            //    Since ArrayList is not generic (Generics are forbidden in this context),
            //    it stores references to the base class 'object'.
            ArrayList logEntries = new ArrayList();

            int epoch = 1;          // Value Type (Int32)
            double loss = 0.045;    // Value Type (Double)
            bool isConverged = false; // Value Type (Boolean)

            Console.WriteLine("--- Boxing Process ---");

            // 2. Boxing: Converting a value type to a reference type.
            //    'epoch' (value) is converted to an 'object' (reference).
            //    The CLR allocates memory on the heap and copies the value.
            object boxedEpoch = epoch; 
            Console.WriteLine($"Boxed Epoch: {boxedEpoch}, Type: {boxedEpoch.GetType()}");

            // 3. Storing in the collection.
            //    Implicit boxing occurs here as 'loss' is converted to 'object'.
            logEntries.Add(loss); 
            logEntries.Add(isConverged);
            logEntries.Add(epoch); // Boxing happens again here.

            Console.WriteLine("\n--- Unboxing Process ---");

            // 4. Unboxing: Explicitly casting the reference type back to the value type.
            //    This retrieves the original value from the heap.
            //    WARNING: This requires an explicit cast.
            int retrievedEpoch = (int)logEntries[3];
            Console.WriteLine($"Unboxed Epoch: {retrievedEpoch}");

            // 5. Accessing the value directly without unboxing (using Object methods).
            //    The value is still boxed, so we can inspect it without unboxing.
            Console.WriteLine($"Direct access to loss: {logEntries[0]}");
        }
    }
}

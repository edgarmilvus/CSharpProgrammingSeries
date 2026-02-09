
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

using System;
using System.Collections.Generic;
using System.Linq;

// Data source
IEnumerable<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

// DEFINING THE PLAN (Deferred Execution)
// No iteration happens here. No filtering. No projection.
// 'query' is just a description of what to do.
var query = numbers.Where(n => n % 2 == 0) // Filter: Keep evens
                   .Select(n => n * 10);    // Project: Multiply by 10

// EXECUTION TRIGGER (Immediate Execution)
// The conveyor belt starts moving only now.
// We iterate over 'query', which triggers the iteration over 'numbers'.
foreach (var item in query)
{
    Console.WriteLine(item); // Output: 20, 40, 50, 60, 70? (Wait, 5 is odd...)
}

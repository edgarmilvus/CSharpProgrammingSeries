
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

// Source File: theory_theoretical_foundations_part5.cs
// Description: Theoretical Foundations
// ==========================================

public class Pipeline
{
    // A method that accepts a list of any type of Number (or subclass)
    // and processes them.
    public void ProcessNumbers(List<? extends Number> numbers)
    {
        foreach (Number n in numbers)
        {
            // We can safely read because the wildcard guarantees it's at least a Number
            Console.WriteLine(n.DoubleValue());
        }
        
        // ERROR: Cannot add to a producer list
        // numbers.Add(10.5); 
    }

    // A method that accepts a list of any super-type of Integer
    // and adds Integers to it.
    public void FillList(List<? super Integer> list)
    {
        // We can safely write because the list accepts Integers or their parents
        list.Add(1);
        list.Add(2);
        
        // ERROR: Cannot safely read (other than Object)
        // Integer i = list.Get(0); // Compilation error
    }
}

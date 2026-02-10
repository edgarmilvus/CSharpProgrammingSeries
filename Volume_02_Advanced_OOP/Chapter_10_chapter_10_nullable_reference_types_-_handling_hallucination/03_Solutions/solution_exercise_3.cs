
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

public class Tensor<T> where T : struct, IEquatable<T>, IFormattable
{
    public T[] Data { get; private set; }
    public int Length => Data.Length;

    public Tensor(T[] data)
    {
        Data = data;
    }

    // Simulated check for invalid floating point numbers
    private bool IsInvalid(T value)
    {
        // In a real generic scenario with modern .NET, we would use INumber<T>.
        // For this intermediate exercise, we handle specific known types.
        if (typeof(T) == typeof(double))
        {
            double d = Convert.ToDouble(value);
            return double.IsNaN(d) || double.IsInfinity(d);
        }
        // For integer types, we assume they are always valid
        return false;
    }

    public Option<Tensor<T>> SafeAdd(Tensor<T> other)
    {
        if (other.Length != this.Length)
        {
            // Dimension mismatch is treated as an invalid operation
            return Option<Tensor<T>>.None();
        }

        T[] resultData = new T[this.Length];

        for (int i = 0; i < this.Length; i++)
        {
            // Check for invalid states in source data
            if (IsInvalid(this.Data[i]) || IsInvalid(other.Data[i]))
            {
                // Found corrupted data. Abort immediately and return None.
                return Option<Tensor<T>>.None();
            }

            // Perform addition (using dynamic for demonstration of logic)
            dynamic a = this.Data[i];
            dynamic b = other.Data[i];
            resultData[i] = a + b;
        }

        return Option<Tensor<T>>.Some(new Tensor<T>(resultData));
    }
}

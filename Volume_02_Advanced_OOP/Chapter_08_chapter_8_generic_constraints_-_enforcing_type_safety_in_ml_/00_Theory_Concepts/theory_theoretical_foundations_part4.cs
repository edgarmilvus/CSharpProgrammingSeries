
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

// Source File: theory_theoretical_foundations_part4.cs
// Description: Theoretical Foundations
// ==========================================

// Custom constraint interface
public interface IValidTensor<T> where T : struct
{
    int Rank { get; }
    void Validate();
}

// Implementation with validation
public class Matrix<T> : IValidTensor<T> where T : struct, IComparable<T>
{
    private T[,] data;
    public int Rank => 2;

    public Matrix(int rows, int cols)
    {
        data = new T[rows, cols];
    }

    public void Validate()
    {
        if (Rank < 2)
            throw new InvalidOperationException("Matrix must have at least 2 dimensions");
        // Additional checks for numeric validity
    }
}

// Pipeline stage using the constraint
public class NormalizationStage<T, TTensor> 
    where T : struct, IComparable<T> 
    where TTensor : IValidTensor<T>, new()
{
    public TTensor Normalize(TTensor input)
    {
        input.Validate(); // Compile-time enforced method call
        // Normalization logic
        return input;
    }
}

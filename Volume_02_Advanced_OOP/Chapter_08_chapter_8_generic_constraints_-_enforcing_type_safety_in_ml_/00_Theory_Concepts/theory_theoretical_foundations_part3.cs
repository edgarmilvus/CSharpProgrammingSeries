
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

// Marker interface for numeric types
public interface INumber<T> where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T> { }

// Implementation for float
public struct FloatNumber : INumber<float> { }

// Tensor shape represented by a struct with dimension fields
public struct Shape2D { public int Rows; public int Cols; }
public struct Shape3D { public int Depth; public int Rows; public int Cols; }

// Constrained tensor class
public class Tensor<T, TShape> where T : struct, INumber<T> where TShape : struct
{
    private T[] data;
    private TShape shape; // The shape is part of the type, but stored as a value

    public Tensor(TShape shape)
    {
        this.shape = shape;
        // Calculate total size based on shape fields (using reflection or switch)
        int size = CalculateSize(shape);
        data = new T[size];
    }

    private int CalculateSize(TShape shape)
    {
        // In real code, we might use pattern matching or a switch on shape type
        if (shape is Shape2D s2d) return s2d.Rows * s2d.Cols;
        if (shape is Shape3D s3d) return s3d.Depth * s3d.Rows * s3d.Cols;
        throw new InvalidOperationException("Unknown shape type");
    }

    // Operation that requires same shape
    public Tensor<T, TShape> Add(Tensor<T, TShape> other)
    {
        // At compile time, we know TShape is the same for both tensors
        // But we still need runtime checks for dimension equality
        if (!ShapeEquals(other.shape))
            throw new InvalidOperationException("Shape mismatch");
        
        // Perform addition (simplified)
        T[] resultData = new T[data.Length];
        for (int i = 0; i < data.Length; i++)
            resultData[i] = (dynamic)data[i] + (dynamic)other.data[i]; // Using dynamic for simplicity; avoid in production

        return new Tensor<T, TShape>(shape) { data = resultData };
    }

    private bool ShapeEquals(TShape other)
    {
        // Runtime shape comparison
        if (shape is Shape2D s1 && other is Shape2D s2)
            return s1.Rows == s2.Rows && s1.Cols == s2.Cols;
        // ... similar for 3D
        return false;
    }
}

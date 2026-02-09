
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

public record Vector(params double[] Coordinates)
{
    public int Dimension => Coordinates.Length;

    // Element-wise addition
    public static Vector operator +(Vector a, Vector b)
    {
        if (a.Dimension != b.Dimension) throw new InvalidOperationException("Dimension mismatch");
        var result = new double[a.Dimension];
        for (int i = 0; i < a.Dimension; i++)
            result[i] = a.Coordinates[i] + b.Coordinates[i];
        return new Vector(result);
    }

    // Element-wise subtraction
    public static Vector operator -(Vector a, Vector b)
    {
        if (a.Dimension != b.Dimension) throw new InvalidOperationException("Dimension mismatch");
        var result = new double[a.Dimension];
        for (int i = 0; i < a.Dimension; i++)
            result[i] = a.Coordinates[i] - b.Coordinates[i];
        return new Vector(result);
    }

    // Scalar multiplication
    public static Vector operator *(Vector v, double scalar)
    {
        var result = new double[v.Dimension];
        for (int i = 0; i < v.Dimension; i++)
            result[i] = v.Coordinates[i] * scalar;
        return new Vector(result);
    }
}

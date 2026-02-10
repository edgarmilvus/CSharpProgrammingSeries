
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

namespace Exercise2
{
    // Step 1 & 4: Struct with individual fields and marked readonly
    public readonly struct Matrix3x3
    {
        // Fields must be readonly for the struct to be readonly
        private readonly float _m00, _m01, _m02;
        private readonly float _m10, _m11, _m12;
        private readonly float _m20, _m21, _m22;

        public Matrix3x3(float[] values)
        {
            // Initialize fields (allowed in constructor)
            _m00 = values[0]; _m01 = values[1]; _m02 = values[2];
            _m10 = values[3]; _m11 = values[4]; _m12 = values[5];
            _m20 = values[6]; _m21 = values[7]; _m22 = values[8];
        }

        // Step 2: Multiply method using 'in'
        public Matrix3x3 Multiply(in Matrix3x3 other)
        {
            // We must return a new instance. 
            // Since this struct is readonly, we cannot modify '_m00' etc. directly.
            return new Matrix3x3(new float[] {
                _m00 * other._m00 + _m01 * other._m10 + _m02 * other._m20,
                _m00 * other._m01 + _m01 * other._m11 + _m02 * other._m21,
                _m00 * other._m02 + _m01 * other._m12 + _m02 * other._m22,
                
                _m10 * other._m00 + _m11 * other._m10 + _m12 * other._m20,
                _m10 * other._m01 + _m11 * other._m11 + _m12 * other._m21,
                _m10 * other._m02 + _m11 * other._m12 + _m12 * other._m22,

                _m20 * other._m00 + _m21 * other._m10 + _m22 * other._m20,
                _m20 * other._m01 + _m21 * other._m11 + _m22 * other._m21,
                _m20 * other._m02 + _m21 * other._m12 + _m22 * other._m22
            });
        }

        // Step 4: Explicitly readonly member (though implied by struct readonly)
        public readonly override string ToString() => $"Matrix(Row1: {_m00},{_m01},{_m02})";

        // Step 5: Edge Case - Indexer
        // An indexer that returns a value is fine, but one that sets (setter) is not allowed
        // in a readonly struct because it would modify the instance.
        public float this[int row, int col]
        {
            get
            {
                return (row, col) switch
                {
                    (0, 0) => _m00, (0, 1) => _m01, (0, 2) => _m02,
                    (1, 0) => _m10, (1, 1) => _m11, (1, 2) => _m12,
                    (2, 0) => _m20, (2, 1) => _m21, (2, 2) => _m22,
                    _ => throw new IndexOutOfRangeException()
                };
            }
            // set { ... } // COMPILER ERROR: The 'struct' is readonly, so 'set' cannot be used.
        }
    }

    public class InferenceEngine
    {
        public void Run()
        {
            var m1 = new Matrix3x3(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var m2 = new Matrix3x3(new float[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 });

            // Step 3: Stack allocation (implicit)
            // The result is a struct returned on the stack.
            Matrix3x3 result = m1.Multiply(in m2); 
        }
    }
}

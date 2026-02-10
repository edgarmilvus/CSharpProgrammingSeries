
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;

public ref struct TokenSentinelScanner
{
    private readonly ReadOnlySpan<byte> _buffer;

    public TokenSentinelScanner(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
    }

    public int FindNextSentinel(ReadOnlySpan<byte> sentinel)
    {
        // Basic linear search implementation
        int maxStart = _buffer.Length - sentinel.Length;
        if (maxStart < 0) return -1;

        for (int i = 0; i <= maxStart; i++)
        {
            bool found = true;
            for (int j = 0; j < sentinel.Length; j++)
            {
                if (_buffer[i + j] != sentinel[j])
                {
                    found = false;
                    break;
                }
            }
            if (found) return i;
        }
        return -1;
    }
}

public static class ValidatorDemonstration
{
    public static void DemonstrateConstraints()
    {
        var buffer = new byte[] { 0x01, 0xFF, 0xFE, 0x02 };
        var scanner = new TokenSentinelScanner(buffer);
        var sentinel = new byte[] { 0xFF, 0xFE };

        // 1. Attempting to Box the struct
        // object boxed = scanner; 
        // ERROR: CS0029: Cannot implicitly convert type 'TokenSentinelScanner' to 'object'
        // REASON: ref structs cannot be boxed because they do not have a method table pointer 
        // and cannot live on the heap, which boxing requires.

        // 2. Attempting to store in a class field
        // class Container { public TokenSentinelScanner Scanner; }
        // ERROR: CS1612: Cannot modify the return value of ... because it is not a variable
        // OR CS0029 depending on context. 
        // REASON: A class field resides on the heap. A ref struct cannot be stored there.

        // 3. Attempting to use as a generic type argument
        // var list = new List<TokenSentinelScanner>();
        // ERROR: CS0305: Using the generic type 'List<T>' requires 1 type arguments
        // REASON: Generics instantiate types on the heap. ref structs are stack-only.

        // 4. Attempting to use in an async method
        // AsyncMethod(scanner);
        // ERROR: CS4013: Instance of type 'TokenSentinelScanner' cannot be used inside an async method or lambda
        // REASON: Async methods lower to a state machine class on the heap. 
        // ref structs cannot be members of heap objects.
    }

    // private async Task AsyncMethod(TokenSentinelScanner scanner) { await Task.Delay(1); }
}

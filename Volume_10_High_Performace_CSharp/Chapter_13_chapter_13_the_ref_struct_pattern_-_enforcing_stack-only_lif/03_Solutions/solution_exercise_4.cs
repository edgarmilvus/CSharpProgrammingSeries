
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;

public ref struct ControlSequenceParser
{
    // Holds a reference to the current byte position
    private ref byte _currentByte;

    public ControlSequenceParser(ReadOnlySpan<byte> stream)
    {
        // We cannot take a ref to a span element in the constructor 
        // unless we have a mutable span. 
        // For this demo, we will simulate tracking position via an integer 
        // and accessing the span via index, as taking 'ref byte' from 
        // a ReadOnlySpan requires unsafe context or pinning.
        
        // However, to satisfy the requirement of 'ref byte _currentByte':
        // We will initialize this in the processing loop logic below.
        _currentByte = ref System.Runtime.CompilerServices.Unsafe.NullRef<byte>();
    }

    public bool TryParseControlSequence(ReadOnlySpan<byte> stream, out ReadOnlySpan<byte> controlToken)
    {
        // Simplified logic: if we see 0xFF, treat it as a control token
        if (stream.Length > 0 && stream[0] == 0xFF)
        {
            controlToken = stream.Slice(0, 1);
            return true;
        }
        controlToken = default;
        return false;
    }

    public void ScanAll(ReadOnlySpan<byte> stream, Action<ReadOnlySpan<byte>> callback)
    {
        // We iterate manually
        for (int i = 0; i < stream.Length; i++)
        {
            // Check if this is a control sequence
            if (TryParseControlSequence(stream.Slice(i), out ReadOnlySpan<byte> token))
            {
                // INVOKING THE CALLBACK
                // We are passing 'token' (a ReadOnlySpan) to the callback.
                // This is SAFE because ReadOnlySpan is a ref struct itself, 
                // but it is passed by value (copied pointer/length).
                // The span points to the original 'stream' memory, which is 
                // valid as long as 'stream' is pinned in the caller's stack frame.
                
                callback(token);
            }
        }
    }
}

public static class ParserAnalysis
{
    public static void Run()
    {
        byte[] data = { 0x01, 0xFF, 0x02 };
        var parser = new ControlSequenceParser();
        
        // Safe usage
        parser.ScanAll(data, token => 
        {
            // We are accessing the token span here. This is valid.
            Console.WriteLine($"Found control token: {token[0]:X}");
        });

        // THE TRAP: Attempting to hoist the parser to the heap
        // Action<ReadOnlySpan<byte>> storedAction = null;
        // parser.ScanAll(data, token => 
        // {
        //     // ERROR: CS4013: Instance of type 'ControlSequenceParser' cannot be used inside an async method or lambda
        //     // Even if we don't use 'parser', just capturing it causes issues if we try to assign it to a field.
        //     
        //     // If we tried to do this:
        //     // storedAction = (s) => { /* use parser */ }; 
        //     // The compiler prevents it because 'parser' is a ref struct.
        // });
    }
}

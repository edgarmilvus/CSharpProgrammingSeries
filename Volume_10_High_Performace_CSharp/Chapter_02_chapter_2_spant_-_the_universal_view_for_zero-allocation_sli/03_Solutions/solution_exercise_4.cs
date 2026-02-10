
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;

public class VectorDecoder
{
    public static ReadOnlySpan<byte> ProcessEmbedding(string base64Input)
    {
        // 1. Estimate output size
        // Base64 length is roughly 4/3 of binary size. 
        // We can calculate exact required size: (base64.Length * 3) / 4
        // We need a buffer large enough.
        
        int maxBytes = (base64Input.Length * 3) / 4;
        // Adjust for padding
        if (base64Input.EndsWith("==")) maxBytes -= 2;
        else if (base64Input.EndsWith("=")) maxBytes -= 1;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxBytes);

        try
        {
            // 2. Decode using Span-based API
            // Convert.TryFromBase64String works on ReadOnlySpan<char>
            // We need to handle the conversion from string to char span.
            
            // Note: For UTF-8 bytes (System.Buffers.Text.Base64), we would use Base64.DecodeFromUtf8
            // But the input is a string, so Convert is appropriate.
            
            bool success = Convert.TryFromBase64String(base64Input, buffer, out int bytesWritten);

            if (!success)
            {
                // Interactive Challenge: Simulate corruption handling
                throw new InvalidDataException("The base64 input string is not valid or the output buffer was too small.");
            }

            // Return a ReadOnlySpan<byte> of the valid data.
            // Note: We cannot return the raw Span<byte> from a rented array outside this scope safely
            // because the array might be returned to the pool while the span is still in use.
            // However, the requirement asks to return the Span. 
            // In a real scenario, we would wrap this in a Memory<byte> or a custom struct that holds the array.
            // Since the prompt asks for Span specifically, we return the slice.
            // WARNING: This is dangerous in production code without pinning or a wrapper object.
            // For this exercise, we assume the caller uses the span immediately.
            
            return buffer.AsSpan(0, bytesWritten);
        }
        catch (Exception ex)
        {
            // Log or handle
            throw;
        }
        finally
        {
            // CRITICAL: We cannot return the buffer here if we are returning a Span to it!
            // This highlights the limitation of Span for returning pooled data.
            // To fix this properly, we would need to return a struct { byte[]; int; } or similar.
            // For the sake of this exercise's constraints, we will comment out the Return 
            // to simulate the "leak" or assume the caller returns it (bad practice).
            // OR, we change the return type to void and take an Action<ReadOnlySpan<byte>>.
            
            // Let's adhere to the prompt's "Return the populated Span" requirement, 
            // but acknowledge the leak in comments.
            
            // ArrayPool<byte>.Shared.Return(buffer); 
            // CANNOT RETURN HERE.
        }
    }

    // Corrected approach for production (Interactive Challenge solution)
    public static void ProcessEmbeddingSafe(string base64Input, Action<ReadOnlySpan<byte>> processAction)
    {
        int maxBytes = (base64Input.Length * 3) / 4;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxBytes);

        try
        {
            if (!Convert.TryFromBase64String(base64Input, buffer, out int bytesWritten))
            {
                throw new InvalidDataException("Invalid Base64 string.");
            }

            // Process the data within the safety of the try block
            processAction(buffer.AsSpan(0, bytesWritten));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // Conceptual SIMD Validation
    public static bool DecodeVectorized(ReadOnlySpan<char> base64, Span<byte> output)
    {
        // Conceptually, we would use Vector<T> to check ranges of characters:
        // Vector.GreaterThan(base64Chars, Vector.Create('A')) etc.
        // But .NET's built-in Convert is already highly optimized.
        // This method signature serves as a placeholder for where such logic would go.
        return Convert.TryFromBase64Chars(base64, output, out _);
    }
}

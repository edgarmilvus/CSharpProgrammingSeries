
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
using System.Text;
using System.Text.Json;

public class HighPerformanceJsonParser
{
    /// <summary>
    /// Parses a UTF-8 JSON object and returns a span of the "text" field value.
    /// </summary>
    /// <param name="utf8Json">The raw UTF-8 bytes of the JSON document.</param>
    /// <returns>A span containing the raw bytes of the text value (excluding quotes).</returns>
    public static ReadOnlySpan<byte> GetTextSpan(ReadOnlySpan<byte> utf8Json)
    {
        var reader = new Utf8JsonReader(utf8Json);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                // Compare the raw bytes of the property name "text"
                // We use Encoding.UTF8.GetBytes for comparison in this example, 
                // but in hot paths, you might compare bytes directly to avoid allocs.
                if (reader.ValueSpan.SequenceEqual("text"u8))
                {
                    // Move to the value token
                    if (!reader.Read()) return ReadOnlySpan<byte>.Empty;

                    if (reader.TokenType == JsonTokenType.String)
                    {
                        // reader.ValueSpan contains the raw UTF-8 bytes including quotes.
                        // We slice off the leading and trailing quotes (1 byte each for ASCII).
                        // This creates a new Span pointing to the inner bytes (zero-copy).
                        return reader.ValueSpan.Slice(1, reader.ValueSpan.Length - 2);
                    }
                }
            }
        }

        return ReadOnlySpan<byte>.Empty;
    }

    public static void Main()
    {
        // UTF-8 encoded JSON
        string json = "{\"id\": 1, \"text\": \"AI Data Engineering\"}";
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(json);

        // Parse without allocating strings
        ReadOnlySpan<byte> textSpan = GetTextSpan(utf8Bytes);

        // Convert only for display purposes
        Console.WriteLine($"Extracted bytes: {Encoding.UTF8.GetString(textSpan)}");
    }
}


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
using System.Buffers;
using System.Collections.Generic;
using System.Text;

public readonly struct LogEntry
{
    public ReadOnlySpan<byte> Timestamp { get; }
    public ReadOnlySpan<byte> Source { get; }
    public ReadOnlySpan<byte> Message { get; }

    public LogEntry(ReadOnlySpan<byte> timestamp, ReadOnlySpan<byte> source, ReadOnlySpan<byte> message)
    {
        Timestamp = timestamp;
        Source = source;
        Message = message;
    }
}

public class ZeroAllocTokenizer
{
    // Using a List<LogEntry> is the ONLY allowed allocation for the final output.
    public static List<LogEntry> ParseLogs(ReadOnlySpan<byte> buffer, byte delimiter = (byte)'|')
    {
        var entries = new List<LogEntry>();
        
        // Iterate through lines using the Split method (allocates an Enumerator, but no string allocations)
        // Note: MemoryExtensions.Split returns a ref struct, so we can't use it in an async method 
        // or iterator block (yield return), so we must process immediately.
        
        // Since we can't yield return from a method containing ref struct usage easily without custom async state machines,
        // we will parse into the List directly.
        
        // We need a custom line iterator to avoid the overhead of Split's enumerator allocation if strict zero-allocation is required,
        // but MemoryExtensions.Split is efficient. Let's use manual parsing for maximum control and SIMD bonus context.
        
        int start = 0;
        while (start < buffer.Length)
        {
            // Find end of line (SIMD bonus context: Vector<byte> could find '\n' faster here)
            int eol = buffer.Slice(start).IndexOf((byte)'\n');
            if (eol == -1) eol = buffer.Length - start;
            
            var line = buffer.Slice(start, eol);
            
            // Handle potential '\r'
            if (line.Length > 0 && line[line.Length - 1] == (byte)'\r')
                line = line.Slice(0, line.Length - 1);

            if (line.Length > 0)
            {
                ParseLine(line, delimiter, entries);
            }

            start += eol + 1;
        }

        return entries;
    }

    private static void ParseLine(ReadOnlySpan<byte> line, byte delimiter, List<LogEntry> entries)
    {
        // Field 1: Timestamp
        int idx1 = line.IndexOf(delimiter);
        if (idx1 == -1) return; // Invalid line

        var timestamp = line.Slice(0, idx1);
        var rest = line.Slice(idx1 + 1);

        // Field 2: Source
        int idx2 = rest.IndexOf(delimiter);
        if (idx2 == -1) 
        {
            // Only 1 delimiter found (missing message)
            var source = rest;
            entries.Add(new LogEntry(timestamp, source, ReadOnlySpan<byte>.Empty));
            return;
        }

        var source = rest.Slice(0, idx2);
        var message = rest.Slice(idx2 + 1);

        // Handle empty fields (e.g., timestamp||message)
        // Note: Spans handle empty slices automatically (length 0).
        
        entries.Add(new LogEntry(timestamp, source, message));
    }
}

// Interactive Challenge Implementation
public class ConfigurableTokenizer
{
    public static List<LogEntry> ParseLogsConfigurable(ReadOnlySpan<byte> buffer, byte delimiter)
    {
        // The logic is identical; the delimiter is just a parameter.
        // This demonstrates that the parsing logic is agnostic to the delimiter value,
        // relying purely on byte comparison.
        return ZeroAllocTokenizer.ParseLogs(buffer, delimiter);
    }
}

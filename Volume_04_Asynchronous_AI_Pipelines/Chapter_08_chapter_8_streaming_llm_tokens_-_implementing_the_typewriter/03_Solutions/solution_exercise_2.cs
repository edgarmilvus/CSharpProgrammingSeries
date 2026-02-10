
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
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class JsonStreamParser
{
    private readonly StringBuilder _buffer = new StringBuilder();
    private int _nestingDepth = 0;
    private bool _insideString = false;
    private bool _escapeNext = false;

    public async IAsyncEnumerable<JsonDocument> ParseTokensAsync(IAsyncEnumerable<string> tokens)
    {
        await foreach (var token in tokens)
        {
            foreach (char c in token)
            {
                ProcessCharacter(c);

                // If depth returns to 0 and we aren't inside a string, we have a complete object
                if (_nestingDepth == 0 && !_insideString && _buffer.Length > 0)
                {
                    var jsonStr = _buffer.ToString();
                    try
                    {
                        var doc = JsonDocument.Parse(jsonStr);
                        yield return doc;
                    }
                    catch (JsonException)
                    {
                        // In a real scenario, we might log this, but here we assume
                        // it's just a partial chunk not yet ready.
                        continue;
                    }
                    finally
                    {
                        _buffer.Clear();
                    }
                }
            }
        }
    }

    private void ProcessCharacter(char c)
    {
        // Handle escaped characters inside strings
        if (_insideString)
        {
            if (_escapeNext)
            {
                _escapeNext = false;
            }
            else if (c == '\\')
            {
                _escapeNext = true;
            }
            else if (c == '"')
            {
                _insideString = false;
            }
            _buffer.Append(c);
            return;
        }

        // Handle non-string content
        switch (c)
        {
            case '"':
                _insideString = true;
                _buffer.Append(c);
                break;
            case '{':
            case '[':
                _nestingDepth++;
                _buffer.Append(c);
                break;
            case '}':
            case ']':
                _nestingDepth--;
                _buffer.Append(c);
                break;
            default:
                _buffer.Append(c);
                break;
        }
    }
}

// Example usage context (not part of the class itself)
public class JsonConsumer
{
    public static async Task ProcessStream(IAsyncEnumerable<string> rawTokens)
    {
        var parser = new JsonStreamParser();
        await foreach (var doc in parser.ParseTokensAsync(rawTokens))
        {
            Console.WriteLine($"Parsed Object: {doc.RootElement.GetRawText()}");
        }
    }
}

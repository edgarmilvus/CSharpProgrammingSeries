
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Columns;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public class TokenData
{
    public int Id { get; set; }
    public string Text { get; set; }
    public float Score { get; set; }
}

[MemoryDiagnoser]
[Config(typeof(PipelineConfig))]
public class SerializationPipelineBenchmark
{
    private string _inputText;
    private List<TokenData> _tokenList;

    private class PipelineConfig : ManualConfig
    {
        public PipelineConfig()
        {
            AddColumn(StatisticColumn.P95);
            AddColumn(StatisticColumn.P99);
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        _inputText = "Token1 123.5 Token2 456.7 Token3 890.1"; // Simulated input
        // Pre-generate list to isolate serialization cost if needed, 
        // but here we simulate the full pipeline processing.
    }

    // Helper to simulate processing (optimized with Span)
    private List<TokenData> ProcessTokens(string input)
    {
        var list = new List<TokenData>();
        ReadOnlySpan<char> span = input.AsSpan();
        int start = 0;
        int idCounter = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == ' ')
            {
                if (i > start)
                {
                    var tokenSpan = span.Slice(start, i - start);
                    // Simulate parsing logic
                    list.Add(new TokenData { Id = idCounter++, Text = tokenSpan.ToString(), Score = (float)new Random().NextDouble() });
                }
                start = i + 1;
            }
        }
        if (start < span.Length)
        {
            var tokenSpan = span.Slice(start);
            list.Add(new TokenData { Id = idCounter++, Text = tokenSpan.ToString(), Score = (float)new Random().NextDouble() });
        }
        return list;
    }

    // 1. Naive JSON Serializer
    private string NaiveSerialize(List<TokenData> tokens)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            sb.Append($"{{\"Id\":{t.Id},\"Text\":\"{t.Text}\",\"Score\":{t.Score}}}");
            if (i < tokens.Count - 1) sb.Append(',');
        }
        sb.Append(']');
        return sb.ToString();
    }

    [Benchmark]
    public string Pipeline_NaiveJson()
    {
        var tokens = ProcessTokens(_inputText);
        return NaiveSerialize(tokens);
    }

    [Benchmark]
    public string Pipeline_SystemTextJson()
    {
        var tokens = ProcessTokens(_inputText);
        return JsonSerializer.Serialize(tokens);
    }

    [Benchmark]
    public string Pipeline_SystemTextJson_Streaming()
    {
        // Using Utf8JsonWriter for zero-allocation writing (conceptually)
        // Note: In a real benchmark, we would write to a pooled buffer.
        // Here we use a MemoryStream for a fair comparison of the API.
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartArray();
            foreach (var t in ProcessTokens(_inputText))
            {
                writer.WriteStartObject();
                writer.WriteNumber("Id", t.Id);
                writer.WriteString("Text", t.Text);
                writer.WriteNumber("Score", t.Score);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<SerializationPipelineBenchmark>();
    }
}

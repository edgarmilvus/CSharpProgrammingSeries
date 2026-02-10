
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

// Program.cs
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseCors();

// Define the record
public record StreamEvent(string Type, string Content, long Timestamp);

app.MapGet("/api/stream/structured", async (CancellationToken ct) =>
{
    async IAsyncEnumerable<string> StreamEvents([EnumeratorCancellation] CancellationToken token)
    {
        // 1. Send Metadata first
        var meta = new StreamEvent("Metadata", "Processing started", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        yield return SerializeEvent(meta);

        await Task.Delay(500, token);

        // 2. Send Tokens
        var tokens = new[] { "Here", " are", " some", " structured", " tokens." };
        foreach (var t in tokens)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(150, token);
            
            var evt = new StreamEvent("Token", t, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            yield return SerializeEvent(evt);
        }

        // 3. Send final metadata
        var endMeta = new StreamEvent("Metadata", "Generation complete", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        yield return SerializeEvent(endMeta);
    }

    // Helper to format as SSE compatible JSON string
    string SerializeEvent(StreamEvent evt)
    {
        // We manually format the "data: " prefix required by SSE
        var json = JsonSerializer.Serialize(evt);
        return $"data: {json}\n\n";
    }

    return Results.Stream(StreamEvents(ct), "text/event-stream");
});

app.Run();

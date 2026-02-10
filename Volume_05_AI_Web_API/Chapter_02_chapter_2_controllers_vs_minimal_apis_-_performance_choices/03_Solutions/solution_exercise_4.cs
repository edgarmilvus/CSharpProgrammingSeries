
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

using System.Runtime.CompilerServices;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
app.UseRouting();
app.MapControllers();

// 1. Mock Streamer
public async IAsyncEnumerable<string> GetTokensAsync()
{
    for (int i = 0; i < 50; i++)
    {
        await Task.Delay(10); // Simulate token generation time
        yield return $"Token_{i} ";
    }
}

// 2. Buffered Endpoint (Controller)
[ApiController]
[Route("api/stream")]
public class StreamController : ControllerBase
{
    [HttpGet("buffered")]
    public async Task<IActionResult> GetBuffered()
    {
        var tokens = new List<string>();
        
        // Buffering all tokens before responding
        await foreach (var token in GetTokensAsync())
        {
            tokens.Add(token);
        }
        
        return Ok(string.Join("", tokens));
    }
}

// 3. Streaming Endpoint (Minimal API)
app.MapGet("/minimal/stream", async () =>
{
    // Results.Stream handles IAsyncEnumerable automatically in .NET 8+
    return Results.Stream(GetTokensAsync(), "text/plain");
});

// 4. Memory Profiling Helper
public class CountingTextWriter : TextWriter
{
    public long BytesWritten { get; private set; }
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value) => BytesWritten += Encoding.GetByteCount(new[] { value });
    public override void Write(string? value) => BytesWritten += Encoding.GetByteCount(value ?? "");
}

// Benchmark endpoint to measure memory/latency
app.MapGet("/benchmark-streaming", async () =>
{
    var stopwatch = Stopwatch.StartNew();
    
    // Simulate buffering
    var bufferedTokens = new List<string>();
    await foreach (var token in GetTokensAsync())
    {
        bufferedTokens.Add(token);
    }
    var bufferedTime = stopwatch.ElapsedMilliseconds;
    
    stopwatch.Restart();
    
    // Simulate streaming (using a counting writer)
    var writer = new CountingTextWriter();
    await foreach (var token in GetTokensAsync())
    {
        await writer.WriteAsync(token);
        // In a real scenario, we would flush to the response stream here
    }
    var streamingTime = stopwatch.ElapsedMilliseconds;
    
    return new 
    { 
        BufferedTimeMs = bufferedTime, 
        StreamingTimeMs = streamingTime,
        Note = "Streaming allows the client to process data earlier, even if total time is similar."
    };
});

app.Run();

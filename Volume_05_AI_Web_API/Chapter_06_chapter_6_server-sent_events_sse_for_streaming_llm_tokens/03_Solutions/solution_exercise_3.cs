
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseCors();

app.MapGet("/api/stream/cancellable", async (CancellationToken ct) =>
{
    async IAsyncEnumerable<string> GenerateStream([EnumeratorCancellation] CancellationToken token)
    {
        try
        {
            // Simulate a long generation (50+ tokens)
            for (int i = 0; i < 60; i++)
            {
                // Check for client disconnect immediately
                token.ThrowIfCancellationRequested();

                // Simulate error after 5 tokens
                if (i == 5)
                {
                    throw new InvalidOperationException("Simulated server error: Model timeout");
                }

                // Pass token to Delay for immediate cancellation
                await Task.Delay(Random.Shared.Next(50, 150), token);

                yield return $"Token {i} ";
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected, just stop silently
            yield break;
        }
        catch (Exception ex)
        {
            // Yield error event before stopping
            // Note: In a real scenario, you might want a specific error format
            var errorJson = System.Text.Json.JsonSerializer.Serialize(new { Type = "Error", Message = ex.Message });
            yield return $"data: {errorJson}\n\n";
        }
    }

    return Results.Stream(GenerateStream(ct), "text/event-stream");
});

app.Run();

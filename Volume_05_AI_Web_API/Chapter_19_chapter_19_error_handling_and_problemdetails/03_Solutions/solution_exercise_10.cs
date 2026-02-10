
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

// Source File: solution_exercise_10.cs
// Description: Solution for Exercise 10
// ==========================================

using System.Runtime.CompilerServices;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatClient _chatClient; // Assume this is injected

    public ChatController(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    [HttpPost("stream")]
    public async Task StreamChat([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        // 1. Check cancellation immediately
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // 2. Attempt to get the stream
            // If this throws before any yield, the global exception handler catches it 
            // and returns a standard 500/ProblemDetails response.
            await foreach (var chunk in GetStreamAsync(request, cancellationToken))
            {
                // 3. Write chunk to response
                // Once we write the first chunk, headers are sent (200 OK).
                await Response.WriteAsync(chunk, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 4. Handle Client Cancellation (Interactive Challenge)
            // The client disconnected. We cannot send a 499 status now.
            // We log this as Information and simply stop processing.
            // In a real scenario, you might write a specific "end-of-stream" marker if protocol allows.
            // Since the stream is already open, we just return (closing the stream cleanly).
            return;
        }
        catch (Exception ex)
        {
            // 5. Mid-stream Error Handling
            // If an error occurs after the first chunk:
            // We cannot change the HTTP status code.
            // We should log the error and attempt to close the stream gracefully.
            
            // Log the error (structured logging as per Ex 4)
            // We cannot return ProblemDetails here because headers are sent.
            // We might write a JSON object indicating error if the client expects it, 
            // or just let the connection close.
            
            // For this exercise, we ensure the exception doesn't bubble up 
            // (which would cause a connection abort).
            // We might log it to a file/stderr since the client won't see it.
            Console.WriteLine($"Stream error after headers sent: {ex.Message}");
        }
    }

    private async IAsyncEnumerable<string> GetStreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        // Simulate streaming response from AI
        var responseStream = _chatClient.GetStreamingResponseAsync(request, ct);

        await foreach (var chunk in responseStream.WithCancellation(ct))
        {
            // Simulate a failure in the middle of the stream
            if (chunk.Contains("Error"))
            {
                throw new InvalidOperationException("AI Service failed mid-stream");
            }
            yield return chunk;
        }
    }
}

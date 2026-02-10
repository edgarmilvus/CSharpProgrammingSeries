
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

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Custom Stream Wrapper to handle interception and immediate flushing
public class StreamingPassThroughStream : Stream
{
    private readonly Stream _innerStream;
    private readonly Stream _originalStream;

    public StreamingPassThroughStream(Stream innerStream, Stream originalStream)
    {
        _innerStream = innerStream;
        _originalStream = originalStream;
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;
    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override void Flush() => _innerStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
    public override void SetLength(long value) => _innerStream.SetLength(value);

    // CRITICAL OVERRIDE: Intercept WriteAsync
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        // Immediately write to the original network stream (Client)
        // This allows "chunked" transfer encoding to work in real-time
        await _originalStream.WriteAsync(buffer, offset, count, cancellationToken);
        
        // Also write to the inner stream (if needed for logging or other logic)
        await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    // Also override the synchronous Write for completeness
    public override void Write(byte[] buffer, int offset, int count)
    {
        _originalStream.Write(buffer, offset, count);
        _innerStream.Write(buffer, offset, count);
    }
}

public class StreamingOptimizedMiddleware
{
    private readonly RequestDelegate _next;

    public StreamingOptimizedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Store the original response body (network stream)
        var originalBody = context.Response.Body;

        // 2. Create a buffer stream (e.g., for logging or temporary holding)
        // In a real scenario, this might be a MemoryStream or a file stream.
        using var bufferStream = new MemoryStream();

        // 3. Replace the response body with our PassThrough wrapper
        // Writes to context.Response.Body will now trigger writes to BOTH streams
        context.Response.Body = new StreamingPassThroughStream(bufferStream, originalBody);

        try
        {
            // 4. Execute the pipeline
            // The AI controller writes to the stream -> PassThroughStream -> Network (immediately)
            await _next(context);
        }
        finally
        {
            // 5. Restore the original stream
            context.Response.Body = originalBody;
        }
    }
}


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

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

// 1. Source Generation Context
[JsonSerializable(typeof(ChatResponseChunk))]
[JsonSerializable(typeof(List<ChatResponseChunk>))]
public partial class ChatJsonContext : JsonSerializerContext { }

public class ChatResponseChunk
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "assistant";

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

// 2. Streaming Logic Service
public class OptimizedChatService
{
    public async Task StreamResponseAsync(IAsyncEnumerable<ChatResponseChunk> chunks, Stream responseStream, CancellationToken cancellationToken)
    {
        // Use a pooled buffer writer to reduce allocations
        using var bufferWriter = new System.Buffers.ArrayBufferWriter<byte>();
        
        await foreach (var chunk in chunks)
        {
            bufferWriter.Clear(); // Reset buffer for new chunk

            // Use Utf8JsonWriter for high-performance serialization
            using (var jsonWriter = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions 
            { 
                Indented = false, // Minimize payload size
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            }))
            {
                // Serialize directly to the buffer
                JsonSerializer.Serialize(jsonWriter, chunk, ChatJsonContext.Default.ChatResponseChunk);
            }

            // 3. SSE Formatting & Writing
            // SSE format: "data: {json}\n\n"
            // We construct the frame directly in UTF-8 bytes to avoid string allocations
            
            var dataPrefix = "data: "u8;
            var newLine = "\n\n"u8;

            // Write "data: "
            await responseStream.WriteAsync(dataPrefix, cancellationToken);
            
            // Write the JSON bytes from the buffer
            await responseStream.WriteAsync(bufferWriter.WrittenMemory, cancellationToken);
            
            // Write "\n\n"
            await responseStream.WriteAsync(newLine, cancellationToken);

            // Flush immediately to send data to client (vital for streaming)
            await responseStream.FlushAsync(cancellationToken);
        }
    }
}

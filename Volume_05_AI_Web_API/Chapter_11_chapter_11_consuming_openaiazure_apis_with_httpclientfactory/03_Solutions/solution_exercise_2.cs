
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

using System.Runtime.CompilerServices;
using System.Text.Json;

// 1. Define the ChatChunk model (provided context)
public class ChatChunk
{
    [System.Text.Json.Serialization.JsonPropertyName("choices")]
    public List<ChunkChoice> Choices { get; set; } = new();

    public class ChunkChoice
    {
        [System.Text.Json.Serialization.JsonPropertyName("delta")]
        public Delta Delta { get; set; } = new();
    }

    public class Delta
    {
        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}

// 2. Implementation inside the OpenAiChatClient (or similar service)
public class OpenAiChatClient
{
    private readonly HttpClient _httpClient;

    public OpenAiChatClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Method to handle streaming
    public async IAsyncEnumerable<ChatChunk> StreamChatCompletionAsync(
        string prompt, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestPayload = new OpenAiRequest
        {
            Messages = new List<OpenAiRequest.Message>
            {
                new OpenAiRequest.Message { Role = "user", Content = prompt }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestPayload), 
            System.Text.Encoding.UTF8, 
            "application/json");

        // 3. Use ResponseHeadersRead to stream immediately
        using var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        // 4. Parse the stream line by line
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Handle SSE format: "data: {json}"
            if (line.StartsWith("data: "))
            {
                var jsonPart = line.Substring(6); // Remove "data: "

                if (jsonPart == "[DONE]") 
                    yield break; // End of stream

                try
                {
                    // 5. Deserialize and yield
                    var chunk = JsonSerializer.Deserialize<ChatChunk>(jsonPart);
                    if (chunk != null)
                    {
                        yield return chunk;
                    }
                }
                catch (JsonException)
                {
                    // Handle malformed JSON chunks gracefully
                    continue;
                }
            }
        }
    }
}

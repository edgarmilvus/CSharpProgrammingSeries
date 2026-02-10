
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

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// 1. Configure Global Options with Source Gen Context
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = ChatJsonSerializerContext.Default;
});

var app = builder.Build();
app.UseRouting();
app.MapControllers();

// 2. Complex Model & Source Generation
public record Message(string Role, string Content);
public record ChatResponse(
    string Role, 
    string Content, 
    List<Message> History, 
    Dictionary<string, object> Metadata);

// Source Generator Definition
[JsonSerializable(typeof(ChatResponse))]
[JsonSerializable(typeof(List<Message>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
internal partial class ChatJsonSerializerContext : JsonSerializerContext { }

// 3. Controller Endpoint
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    [HttpGet]
    public IActionResult GetChat()
    {
        var response = new ChatResponse(
            "assistant", 
            "Here is your optimized response.", 
            new List<Message> { new Message("user", "Hello") }, 
            new Dictionary<string, object> { { "tokens", 50 } });
        
        return Ok(response); // Uses global Source Gen context
    }
}

// 4. Minimal API Endpoint
app.MapGet("/minimal/chat", () =>
{
    var response = new ChatResponse(
        "assistant", 
        "Here is your optimized response.", 
        new List<Message> { new Message("user", "Hello") }, 
        new Dictionary<string, object> { { "tokens", 50 } });

    // Explicitly use Source Gen context for this endpoint
    return Results.Json(response, options: new JsonSerializerOptions
    {
        TypeInfoResolver = ChatJsonSerializerContext.Default
    });
});

// 5. Benchmarking Endpoint
app.MapGet("/benchmark-serialization", () =>
{
    var response = new ChatResponse(
        "assistant", "test", 
        Enumerable.Repeat(new Message("user", "msg"), 100).ToList(), 
        new Dictionary<string, object>());
    
    var stopwatch = Stopwatch.StartNew();
    
    // 10,000 iterations
    for (int i = 0; i < 10_000; i++)
    {
        // Force serialization to string to measure pure CPU cost
        JsonSerializer.Serialize(response, ChatJsonSerializerContext.Default.ChatResponse);
    }
    
    stopwatch.Stop();
    return $"Serialization Time: {stopwatch.ElapsedMilliseconds}ms";
});

app.Run();


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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

// 1. Custom Attribute to mark streaming endpoints
[AttributeUsage(AttributeTargets.Method)]
public class SwaggerStreamingAttribute : Attribute { }

// 2. Custom Operation Filter
public class StreamingResponseFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has our custom attribute
        var hasStreamingAttr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerStreamingAttribute), false).Any();
        if (!hasStreamingAttr) return;

        // Target the 200 OK response
        if (operation.Responses.ContainsKey("200"))
        {
            var response = operation.Responses["200"];
            
            // Clear existing content (which might try to describe a complex object)
            response.Content.Clear();

            // Add a text-based content type suitable for NDJSON or SSE
            response.Content.Add("application/x-ndjson", new OpenApiMediaType
            {
                Schema = new OpenApiSchema { Type = "string", Format = "binary" },
                Example = new OpenApiString(
                    "{\"id\": 1, \"content\": \"Hello\"}\n" +
                    "{\"id\": 2, \"content\": \"World\"}")
            });

            // Update the description
            response.Description = "A continuous stream of newline-delimited JSON objects.";
        }
    }
}

// 3. Chat Controller
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    /// <summary>
    /// Streams chat responses using Server-Sent Events (NDJSON).
    /// </summary>
    [HttpPost("stream")]
    [SwaggerStreaming] // Apply our custom marker
    [SwaggerResponse(StatusCodes.Status200OK, "Stream of chat chunks")]
    public async IAsyncEnumerable<string> StreamChat()
    {
        // Mock streaming data
        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(100); // Simulate processing time
            var chunk = $"{{\"chunk\": \"Message {i}\"}}\n";
            yield return chunk;
        }
    }
}

// --- Program.cs Configuration ---
// builder.Services.AddSwaggerGen(c => 
// {
//     c.OperationFilter<StreamingResponseFilter>();
// });

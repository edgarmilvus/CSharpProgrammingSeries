
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

using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IChatService, MockChatService>();

var app = builder.Build();
app.UseRouting();
app.MapControllers();

// --- BEFORE: Controller Implementation ---

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService) => _chatService = chatService;

    [HttpPost]
    [ServiceFilter(typeof(ChatValidationFilter))] // Custom ActionFilter
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, [FromQuery] bool stream = false)
    {
        var response = await _chatService.GetResponse(request.Prompt);
        return Ok(new { response });
    }
}

// Action Filter (Old Way)
public class ChatValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.ActionArguments.Values.OfType<ChatRequest>().FirstOrDefault();
        if (request != null && string.IsNullOrWhiteSpace(request.Prompt))
        {
            context.Result = new BadRequestObjectResult("Prompt cannot be empty.");
        }
    }
    public void OnActionExecuted(ActionExecutedContext context) { }
}

// --- AFTER: Minimal API Refactor ---

app.MapPost("/minimal/chat", async (ChatRequest request, IChatService chatService) =>
{
    var response = await chatService.GetResponse(request.Prompt);
    return Results.Ok(new { response });
})
.WithName("ChatCompletion") // OpenAPI OperationId
.WithOpenApi() // Generates OpenAPI schema
.AddEndpointFilter<ChatEndpointFilter>(); // Custom EndpointFilter

// Endpoint Filter (New Way)
public class ChatEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.GetArgument<ChatRequest>(0); // Get first arg
        
        if (string.IsNullOrWhiteSpace(request?.Prompt))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Prompt", new[] { "Prompt cannot be empty." } }
            });
        }

        return await next(context);
    }
}

// --- Supporting Models ---
public record ChatRequest(string Prompt);
public interface IChatService { Task<string> GetResponse(string prompt); }
public class MockChatService : IChatService { public Task<string> GetResponse(string p) => Task.FromResult("Response"); }

app.Run();

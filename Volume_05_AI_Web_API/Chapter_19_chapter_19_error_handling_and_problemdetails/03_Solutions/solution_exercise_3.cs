
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

// 1. The Data Model
public class ChatRequest
{
    [Required(ErrorMessage = "Prompt is mandatory.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Prompt must be between 1 and 500 characters.")]
    public string Prompt { get; set; }

    [Range(0, 2, ErrorMessage = "Temperature must be between 0 and 2.")]
    public float Temperature { get; set; }
}

// 2. The Action Filter
public class ValidationProblemDetailsFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var detailBuilder = new StringBuilder();
            foreach (var error in errors)
            {
                detailBuilder.AppendLine(error);
            }

            var problemDetails = new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Validation Failed",
                Status = 400,
                Detail = detailBuilder.ToString().Trim()
            };

            context.Result = new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

// 3. Controller to test the filter
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    [HttpPost("validate")]
    public IActionResult ValidateChat([FromBody] ChatRequest request)
    {
        // If we reach here, the filter has passed validation
        return Ok(new { message = "Validation passed" });
    }
}


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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

// 1. Enums and Models
public enum JobStatusEnum { Pending, Running, Completed, Failed }

public class JobStatus
{
    public Guid Id { get; set; }
    public JobStatusEnum Status { get; set; }
    
    // Conditional result
    public string? Result { get; set; } 
}

// 2. Custom Operation Filter for "Retry-After"
public class RetryAfterHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the operation has a 202 response defined
        if (operation.Responses.TryGetValue("202", out var response))
        {
            // Add the Retry-After header definition
            response.Headers ??= new Dictionary<string, OpenApiHeader>();
            
            if (!response.Headers.ContainsKey("Retry-After"))
            {
                response.Headers.Add("Retry-After", new OpenApiHeader
                {
                    Description = "Delay in seconds before polling again.",
                    Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
                });
            }
        }
    }
}

// 3. Job Controller
[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    /// <summary>
    /// Submits a new inference job for processing.
    /// </summary>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Job accepted for processing.", typeof(void))]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "Location", "string", "URL to check job status.")]
    public IActionResult SubmitJob()
    {
        var jobId = Guid.NewGuid();
        var locationUrl = $"/api/jobs/{jobId}";

        // Return 202 with Location header
        return Accepted(locationUrl);
    }

    /// <summary>
    /// Checks the status of a submitted job.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Current job status.", typeof(JobStatus))]
    public IActionResult GetStatus(Guid id)
    {
        // Mock response
        var status = new JobStatus
        {
            Id = id,
            Status = JobStatusEnum.Completed,
            Result = "Inference result here"
        };
        return Ok(status);
    }
}

// --- Configuration in Program.cs ---
// builder.Services.AddSwaggerGen(c => 
// {
//     c.OperationFilter<RetryAfterHeaderFilter>();
// });

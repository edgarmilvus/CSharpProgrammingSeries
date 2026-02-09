
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

# Source File: solution_exercise_6.cs
# Description: Solution for Exercise 6
# ==========================================

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LegacyPlugin.Controllers
{
    [ApiController]
    [Route("api/plugins")]
    public class PluginController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public PluginController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("execute")]
        [Consumes("application/json")]
        public async Task<IActionResult> Execute([FromBody] FunctionCallBatchRequest request)
        {
            if (request?.Calls == null || !request.Calls.Any())
            {
                return BadRequest("Invalid request format. Expected a list of function calls.");
            }

            var results = new List<FunctionResult>();

            foreach (var call in request.Calls)
            {
                try
                {
                    if (call.FunctionName == "lookup_employee")
                    {
                        // Extract arguments
                        if (call.Arguments.TryGetValue("id", out var idObj))
                        {
                            var id = idObj?.ToString(); // Handle JSON element or string
                            var employee = await _employeeService.GetByIdAsync(id);
                            
                            results.Add(new FunctionResult 
                            { 
                                Output = JsonSerializer.Serialize(employee) 
                            });
                        }
                        else
                        {
                            results.Add(new FunctionResult 
                            { 
                                Output = "Error: Missing 'id' argument.",
                                IsError = true 
                            });
                        }
                    }
                    else
                    {
                        results.Add(new FunctionResult 
                        { 
                            Output = $"Error: Unknown function '{call.FunctionName}'.",
                            IsError = true 
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Handle partial failures (Interactive Challenge)
                    results.Add(new FunctionResult 
                    { 
                        Output = $"Error processing '{call.FunctionName}': {ex.Message}",
                        IsError = true 
                    });
                }
            }

            return Ok(new { results = results });
        }
    }

    // Models
    public class FunctionCallBatchRequest
    {
        public List<FunctionCallRequest> Calls { get; set; }
    }

    public class FunctionCallRequest
    {
        public string FunctionName { get; set; }
        public Dictionary<string, object> Arguments { get; set; } // Flexible for JSON objects
    }

    public class FunctionResult
    {
        public string Output { get; set; }
        public bool IsError { get; set; } = false;
    }
}

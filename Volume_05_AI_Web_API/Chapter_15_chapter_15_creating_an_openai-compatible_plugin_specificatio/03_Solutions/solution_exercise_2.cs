
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

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace LegacyPlugin.Filters
{
    public class AiPluginDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // 1. Filter Paths: Keep only GET /api/v1/employees/{id}
            var allowedPaths = swaggerDoc.Paths
                .Where(p => p.Key.Contains("/api/v1/employees/{id}") && p.Value.Operations.ContainsKey(OperationType.Get))
                .ToDictionary(p => p.Key, p => p.Value);

            swaggerDoc.Paths = allowedPaths;

            // 2. Ensure the parameter exists and is required
            if (swaggerDoc.Paths.TryGetValue("/api/v1/employees/{id}", out var pathItem))
            {
                var operation = pathItem.Operations[OperationType.Get];
                
                // Explicitly define the ID parameter
                var idParam = operation.Parameters.FirstOrDefault(p => p.Name == "id");
                if (idParam != null)
                {
                    idParam.Required = true;
                    idParam.Schema.Type = "string"; // Or integer based on domain
                }

                // 3. Inject Pagination Parameters (Interactive Challenge)
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "skip",
                    In = ParameterLocation.Query,
                    Description = "Number of records to skip",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "integer", Default = 0 }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "take",
                    In = ParameterLocation.Query,
                    Description = "Number of records to take",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "integer", Default = 10 }
                });

                // 4. Define 200 Response Schema
                operation.Responses.Clear();
                operation.Responses.Add("200", new OpenApiResponse
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["id"] = new OpenApiSchema { Type = "string", Example = "123" },
                                    ["name"] = new OpenApiSchema { Type = "string", Example = "John Doe" },
                                    ["department"] = new OpenApiSchema { Type = "string", Example = "Engineering" }
                                }
                            }
                        }
                    }
                });
            }

            // 5. Remove definitions not used to keep the schema clean
            // (Optional cleanup step for strict AI schemas)
            var schemasToRemove = swaggerDoc.Components.Schemas
                .Where(kvp => !kvp.Key.Contains("Employee")) // Keep Employee related
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in schemasToRemove)
            {
                swaggerDoc.Components.Schemas.Remove(key);
            }
        }
    }
}


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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AIChatSwaggerSimulator
{
    // REAL-WORLD CONTEXT:
    // In a production AI API, we often need to simulate how Swagger documents
    // complex request payloads without actually hosting a web server. This console
    // application simulates the generation of OpenAPI JSON schemas for an AI Chat API,
    // focusing on how Swagger handles nested objects (ChatMessage), arrays (Conversation History),
    // and enums (ChatRole). This is crucial for developers to understand how their API
    // documentation will appear to consumers.

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== AI Chat API Swagger Documentation Simulator ===");
            Console.WriteLine("Simulating OpenAPI Schema Generation for Chat Endpoints...\n");

            // 1. Initialize the Schema Generator
            // This mimics the SwaggerGenerator configured in ASP.NET Core's Startup.cs
            OpenApiSchemaGenerator schemaGenerator = new OpenApiSchemaGenerator();

            // 2. Generate Schema for a Complex Request Payload
            // We are documenting a POST request to /api/chat with a JSON body.
            string chatRequestSchema = schemaGenerator.GenerateSchema(typeof(ChatCompletionRequest));

            Console.WriteLine("--- Generated OpenAPI Schema (JSON) ---");
            Console.WriteLine(chatRequestSchema);
            Console.WriteLine();

            // 3. Simulate Documentation of Streaming Response
            // AI APIs often return text/event-stream. Swagger documents this as a response schema.
            string streamingResponseSchema = schemaGenerator.GenerateStreamingResponseSchema();

            Console.WriteLine("--- Streaming Response Documentation ---");
            Console.WriteLine(streamingResponseSchema);
            Console.WriteLine();

            // 4. Security Definition Simulation
            // APIs require authentication. Swagger defines this in the 'securityDefinitions' section.
            string securitySchema = schemaGenerator.GenerateSecuritySchema();

            Console.WriteLine("--- Security Definitions (Bearer Auth) ---");
            Console.WriteLine(securitySchema);
            Console.WriteLine();

            Console.WriteLine("Simulation Complete. Review the schemas above to understand API contract.");
        }
    }

    // ==========================================
    // DATA MODELS (Mimicking DTOs)
    // ==========================================

    /// <summary>
    /// Represents the role of the sender in a chat message.
    /// This enum is used by Swagger to restrict values to specific strings.
    /// </summary>
    public enum ChatRole
    {
        System,
        User,
        Assistant
    }

    /// <summary>
    /// Represents a single message in the chat history.
    /// Swagger will document this as a complex object with properties.
    /// </summary>
    public class ChatMessage
    {
        public ChatRole Role { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// Represents the request payload for generating a chat completion.
    /// This includes an array of messages and configuration parameters.
    /// </summary>
    public class ChatCompletionRequest
    {
        public string Model { get; set; }
        public List<ChatMessage> Messages { get; set; }
        public float Temperature { get; set; }
        public bool Stream { get; set; }
    }

    // ==========================================
    // CORE LOGIC: SWAGGER SCHEMA GENERATOR
    // ==========================================

    /// <summary>
    /// Simulates the Swagger/OpenAPI schema generation process.
    /// In a real ASP.NET Core app, Swashbuckle or NSwag does this automatically via Reflection.
    /// Here, we manually construct the JSON strings to demonstrate the underlying structure.
    /// </summary>
    public class OpenApiSchemaGenerator
    {
        // StringBuilder is used for efficient string concatenation, avoiding
        // the performance hit of immutable string operations in loops.
        private StringBuilder _builder;

        public OpenApiSchemaGenerator()
        {
            _builder = new StringBuilder();
        }

        /// <summary>
        /// Generates a JSON schema representation for a given Type.
        /// This mimics how Swagger inspects C# classes to create OpenAPI definitions.
        /// </summary>
        public string GenerateSchema(Type type)
        {
            _builder.Clear();
            _builder.Append("{\n");
            _builder.Append("  \"type\": \"object\",\n");
            _builder.Append("  \"properties\": {\n");

            // We use reflection-like logic (manual mapping here) to inspect properties.
            // In a real app, Swashbuckle uses System.Reflection to get properties.
            var properties = type.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                string propName = prop.Name;
                string propType = MapToOpenApiType(prop.PropertyType.Name);
                string jsonPropType = MapToJsonType(prop.PropertyType.Name);

                _builder.Append($"    \"{propName}\": {{\n");
                _builder.Append($"      \"type\": \"{jsonPropType}\",\n");

                // Handle complex nested objects (like ChatMessage or List<ChatMessage>)
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    if (prop.PropertyType.IsGenericType) // Handling List<T>
                    {
                        _builder.Append("      \"items\": {\n");
                        _builder.Append($"        \"$ref\": \"#/definitions/{prop.PropertyType.GetGenericArguments()[0].Name}\"\n");
                        _builder.Append("      },\n");
                    }
                    else
                    {
                        _builder.Append($"      \"$ref\": \"#/definitions/{prop.PropertyType.Name}\",\n");
                    }
                }

                // Handle Enums (ChatRole)
                if (prop.PropertyType.IsEnum)
                {
                    _builder.Append("      \"enum\": [");
                    string[] enumNames = Enum.GetNames(prop.PropertyType);
                    for (int j = 0; j < enumNames.Length; j++)
                    {
                        _builder.Append($"\"{enumNames[j]}\"");
                        if (j < enumNames.Length - 1) _builder.Append(", ");
                    }
                    _builder.Append("]\n");
                }
                else
                {
                    _builder.Append("      \"description\": \"The " + propName + " property.\"\n");
                }

                _builder.Append("    }");
                if (i < properties.Length - 1) _builder.Append(",");
                _builder.Append("\n");
            }

            _builder.Append("  }\n");
            _builder.Append("}");
            
            return _builder.ToString();
        }

        /// <summary>
        /// Generates documentation for a streaming response.
        /// AI APIs often return NDJSON (Newline Delimited JSON) or Server-Sent Events (SSE).
        /// Swagger documents this using the 'text/event-stream' media type.
        /// </summary>
        public string GenerateStreamingResponseSchema()
        {
            _builder.Clear();
            _builder.Append("{\n");
            _builder.Append("  \"description\": \"Server-Sent Events stream of chat tokens.\",\n");
            _builder.Append("  \"content\": {\n");
            _builder.Append("    \"text/event-stream\": {\n");
            _builder.Append("      \"schema\": {\n");
            _builder.Append("        \"type\": \"string\",\n");
            _builder.Append("        \"example\": \"data: {\\\"id\\\": \\\"chatcmpl-123\\\", \\\"object\\\": \\\"chat.completion.chunk\\\"}\\n\\n\"\n");
            _builder.Append("      }\n");
            _builder.Append("    }\n");
            _builder.Append("  }\n");
            _builder.Append("}");
            return _builder.ToString();
        }

        /// <summary>
        /// Generates the Security Definitions block.
        /// This tells Swagger UI to add an "Authorize" button.
        /// </summary>
        public string GenerateSecuritySchema()
        {
            _builder.Clear();
            _builder.Append("{\n");
            _builder.Append("  \"securityDefinitions\": {\n");
            _builder.Append("    \"Bearer\": {\n");
            _builder.Append("      \"type\": \"apiKey\",\n");
            _builder.Append("      \"name\": \"Authorization\",\n");
            _builder.Append("      \"in\": \"header\",\n");
            _builder.Append("      \"description\": \"JWT Token required for AI model access.\"\n");
            _builder.Append("    }\n");
            _builder.Append("  }\n");
            _builder.Append("}");
            return _builder.ToString();
        }

        // Helper methods to map C# types to OpenAPI/JSON types
        private string MapToOpenApiType(string csharpType)
        {
            if (csharpType == "String" || csharpType == "Guid") return "string";
            if (csharpType == "Int32" || csharpType == "Int64") return "integer";
            if (csharpType == "Boolean") return "boolean";
            if (csharpType == "Double" || csharpType == "Float") return "number";
            if (csharpType.Contains("List")) return "array";
            return "object";
        }

        private string MapToJsonType(string csharpType)
        {
            if (csharpType == "Boolean") return "boolean";
            if (csharpType == "String") return "string";
            if (csharpType == "Double" || csharpType == "Float") return "number";
            if (csharpType.Contains("List")) return "array";
            return "object";
        }
    }
}

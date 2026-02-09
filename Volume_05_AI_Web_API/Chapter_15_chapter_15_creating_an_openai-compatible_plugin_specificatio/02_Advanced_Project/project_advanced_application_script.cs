
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

namespace AdvancedPluginScript
{
    // ---------------------------------------------------------
    // REAL-WORLD CONTEXT
    // ---------------------------------------------------------
    // Scenario: A Logistics Management System.
    // Problem: A human operator needs to quickly check the status of shipments
    // and calculate estimated delivery times. Doing this manually via a UI is slow.
    // Solution: We build a "Plugin Controller" logic that mimics the structure
    // required by OpenAI's plugin specification. This console app simulates
    // the backend logic that would be exposed as an API endpoint.
    //
    // This code demonstrates the "Advanced Application Script" logic:
    // Parsing structured input, validating parameters against a schema
    // (simulated), performing business logic, and returning a structured
    // response compatible with an LLM function call.
    // ---------------------------------------------------------

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Advanced Plugin Script Simulator ===");
            Console.WriteLine("Simulating OpenAI Plugin Function Calling Logic...\n");

            // 1. Simulate incoming JSON payload from an LLM (e.g., ChatGPT)
            // In a real ASP.NET Core API, this would come from the HTTP Request Body.
            string llmPayload = "{\"function\": \"calculate_eta\", \"arguments\": {\"shipment_id\": \"SHP-998877\", \"current_location\": \"New York\"}}";

            Console.WriteLine($"[INPUT] Received Payload: {llmPayload}");

            // 2. Parse the Payload
            // We use basic string manipulation to simulate JSON parsing without external libraries.
            PluginRequest request = ParseRequest(llmPayload);

            // 3. Route to the appropriate Plugin Function
            // This mimics the routing logic in an ASP.NET Core Controller.
            string response = ExecutePluginFunction(request);

            Console.WriteLine($"\n[OUTPUT] LLM Compatible Response:\n{response}");
        }

        // ---------------------------------------------------------
        // LOGIC BLOCK 1: Request Parsing
        // ---------------------------------------------------------
        // Objective: Extract the function name and arguments from a raw string.
        // Constraint: Using basic string methods (IndexOf, Substring) instead of
        // System.Text.Json to adhere to the "basic blocks" rule.
        static PluginRequest ParseRequest(string rawJson)
        {
            PluginRequest request = new PluginRequest();

            // Extract Function Name
            int funcStart = rawJson.IndexOf("\"function\":") + 12;
            int funcEnd = rawJson.IndexOf("\"", funcStart);
            request.FunctionName = rawJson.Substring(funcStart, funcEnd - funcStart);

            // Extract Arguments (Simplified parsing for the demo)
            // In production, use a robust JSON parser.
            int argsStart = rawJson.IndexOf("{", rawJson.IndexOf("\"arguments\""));
            int argsEnd = rawJson.LastIndexOf("}") + 1;
            string argsBlock = rawJson.Substring(argsStart, argsEnd - argsStart);
            
            // Store raw arguments for the specific function handler to parse
            request.RawArguments = argsBlock;

            return request;
        }

        // ---------------------------------------------------------
        // LOGIC BLOCK 2: Function Dispatcher
        // ---------------------------------------------------------
        // Objective: Route the request to the correct business logic handler.
        // Mimics the [HttpGet] or [HttpPost] attributes in ASP.NET Core.
        static string ExecutePluginFunction(PluginRequest request)
        {
            string result = "";

            // Switch statement for routing (Basic control flow)
            switch (request.FunctionName)
            {
                case "get_shipment_status":
                    result = HandleGetStatus(request.RawArguments);
                    break;
                
                case "calculate_eta":
                    result = HandleCalculateETA(request.RawArguments);
                    break;

                default:
                    result = FormatError($"Unknown function: {request.FunctionName}");
                    break;
            }

            return result;
        }

        // ---------------------------------------------------------
        // LOGIC BLOCK 3: Business Logic - Get Status
        // ---------------------------------------------------------
        // Objective: Simulate database lookup for shipment status.
        // Validates input parameters (ID) before processing.
        static string HandleGetStatus(string argsJson)
        {
            // 1. Parameter Extraction & Validation (Simulated Schema Validation)
            string shipmentId = ExtractArgument(argsJson, "shipment_id");
            
            if (string.IsNullOrEmpty(shipmentId))
            {
                return FormatError("Missing required parameter: shipment_id");
            }

            // 2. Mock Database Lookup
            // In a real app, this would query Entity Framework Core or SQL.
            string status = "In Transit"; // Mock data
            string location = "Distribution Center A";

            // 3. Construct Response Object
            // This structure matches the OpenAPI schema definition for the response.
            var responseObj = new
            {
                shipment_id = shipmentId,
                status = status,
                current_location = location,
                last_updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return SerializeObject(responseObj);
        }

        // ---------------------------------------------------------
        // LOGIC BLOCK 4: Business Logic - Calculate ETA
        // ---------------------------------------------------------
        // Objective: Perform calculation based on input coordinates.
        // Demonstrates complex logic within a plugin endpoint.
        static string HandleCalculateETA(string argsJson)
        {
            // 1. Parameter Extraction
            string shipmentId = ExtractArgument(argsJson, "shipment_id");
            string currentLocation = ExtractArgument(argsJson, "current_location");

            if (string.IsNullOrEmpty(shipmentId) || string.IsNullOrEmpty(currentLocation))
            {
                return FormatError("Missing required parameters: shipment_id and current_location");
            }

            // 2. Business Logic (Simulated)
            // Calculate distance based on location string length (mock logic)
            int distanceFactor = currentLocation.Length * 10; 
            int hours = distanceFactor / 50; // Mock speed
            if (hours < 1) hours = 1;

            DateTime arrivalTime = DateTime.Now.AddHours(hours);

            // 3. Construct Response
            var responseObj = new
            {
                shipment_id = shipmentId,
                estimated_arrival = arrivalTime.ToString("yyyy-MM-dd HH:mm:ss"),
                estimated_hours = hours,
                confidence = "High"
            };

            return SerializeObject(responseObj);
        }

        // ---------------------------------------------------------
        // HELPER METHODS (Utility Logic)
        // ---------------------------------------------------------
        
        // Simulates JSON property extraction
        static string ExtractArgument(string json, string key)
        {
            string searchKey = "\"" + key + "\":";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1) return null;

            int valueStart = keyIndex + searchKey.Length;
            // Handle quoted strings
            if (json[valueStart] == '"')
            {
                valueStart++;
                int valueEnd = json.IndexOf('"', valueStart);
                return json.Substring(valueStart, valueEnd - valueStart);
            }
            // Handle numbers (simplified)
            else
            {
                int valueEnd = json.IndexOfAny(new char[] { ',', '}' }, valueStart);
                return json.Substring(valueStart, valueEnd - valueStart);
            }
        }

        // Simulates JSON serialization
        static string SerializeObject(object obj)
        {
            // In a real app, return System.Text.Json.JsonSerializer.Serialize(obj);
            // Here we manually construct a JSON-like string for demonstration.
            var sb = new StringBuilder();
            sb.Append("{ ");
            
            var props = obj.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var val = prop.GetValue(obj);
                sb.Append($"\"{prop.Name}\": \"{val}\"");
                if (i < props.Length - 1) sb.Append(", ");
            }
            
            sb.Append(" }");
            return sb.ToString();
        }

        static string FormatError(string message)
        {
            return $"{{ \"error\": \"{message}\" }}";
        }
    }

    // ---------------------------------------------------------
    // DATA MODELS
    // ---------------------------------------------------------
    // Simple class to hold the parsed request.
    // In ASP.NET Core, this would be a DTO (Data Transfer Object).
    public class PluginRequest
    {
        public string FunctionName { get; set; }
        public string RawArguments { get; set; }
    }
}


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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.Rest;

namespace ApiPluginConverter
{
    // Real-world context: We are building an AI Agent for a "Smart Home Control Center".
    // The agent needs to interact with a legacy thermostat API that uses a specific JSON structure
    // and Basic Authentication. The goal is to convert this raw API into a Semantic Kernel Plugin
    // so the AI can naturally say "Set the living room temperature to 72 degrees" without knowing
    // the underlying HTTP details.

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Initialize the Kernel
            // In a real scenario, we would use Azure OpenAI or OpenAI. 
            // For this console app, we will mock the kernel to focus on the Plugin architecture.
            var kernel = new KernelBuilder().Build();
            
            // 2. Define the Legacy API Details (Simulating an OpenAPI spec or manual config)
            // This represents the "Convert Existing APIs" concept.
            string baseUrl = "https://api.legacy-smarthome.com/v1";
            string endpointPath = "/thermostat/target";
            
            // 3. Create the Authentication Provider
            // Handling authentication is a key part of converting APIs to plugins.
            var authProvider = new BasicAuthenticationProvider("user", "password");
            
            // 4. Build the RestApiOperation manually (Simulating the parser)
            // We map natural language parameters to HTTP parameters.
            var parameters = new List<RestApiParameter>
            {
                new RestApiParameter(
                    name: "temperature",
                    type: typeof(double),
                    description: "The target temperature in Fahrenheit",
                    location: RestApiParameterLocation.Query,
                    isRequired: true
                ),
                new RestApiParameter(
                    name: "room",
                    type: typeof(string),
                    description: "The room name",
                    location: RestApiParameterLocation.Path,
                    isRequired: true
                )
            };

            // 5. Create the Semantic Wrapper (The Plugin)
            // We wrap the raw HTTP operation in a semantic function that the AI can understand.
            // This handles the translation from Natural Language -> Structured API Call.
            var thermostatPlugin = CreateThermostatPlugin(kernel, baseUrl, endpointPath, authProvider, parameters);

            // 6. Register the Plugin
            kernel.ImportPluginFromObject(thermostatPlugin, "Thermostat");

            // 7. Simulate an AI Agent Request
            Console.WriteLine("Agent Request: 'Set the living room temperature to 72 degrees'");
            
            // In a real execution, the Kernel's planner would invoke this function.
            // Here, we manually invoke the semantic wrapper to demonstrate the logic.
            var result = await ExecuteThermostatCommand(kernel, "living room", 72.0);

            Console.WriteLine($"\nAPI Execution Result: {result}");
        }

        // --- Architecture Block: The Semantic Wrapper ---
        // This method creates a semantic function that acts as the bridge between
        // the AI's intent and the legacy API's strict requirements.
        static object CreateThermostatPlugin(
            Kernel kernel, 
            string baseUrl, 
            string path, 
            BasicAuthenticationProvider authProvider, 
            List<RestApiParameter> parameters)
        {
            // We define a function prompt that instructs the AI (or the system)
            // how to map inputs to the API structure.
            string prompt = @"
                Convert the user's request into a structured API call to the legacy thermostat system.
                Inputs: Room Name (string) and Temperature (double).
                Output: A JSON object containing 'url', 'method', 'auth', and 'body'.
            ";

            // Define the function configuration
            var functionConfig = new KernelFunctionConfig(prompt)
                .AddParameter("room", typeof(string))
                .AddParameter("temperature", typeof(double));

            // Create the function
            var function = kernel.CreateFunctionFromPrompt(functionConfig);
            
            // Return a wrapper object that holds the function and API details
            return new ThermostatWrapper(function, baseUrl, path, authProvider, parameters);
        }

        // --- Architecture Block: The Execution Engine ---
        // This simulates the RestApiOperationRunner and the Kernel's execution pipeline.
        static async Task<string> ExecuteThermostatCommand(Kernel kernel, string room, double temperature)
        {
            // Retrieve the plugin
            var plugin = kernel.Plugins["Thermostat"];
            var wrapper = plugin.Metadata as ThermostatWrapper;
            
            if (wrapper == null) return "Error: Plugin not found.";

            // 1. Semantic Mapping (Natural Language -> Structured Data)
            // The AI/Kernel extracts the parameters from the user's request.
            var arguments = new KernelArguments
            {
                ["room"] = room,
                ["temperature"] = temperature
            };

            // 2. Invoke the Semantic Wrapper
            // This executes the prompt logic defined in CreateThermostatPlugin.
            var result = await kernel.InvokeAsync(plugin["SetTemperature"], arguments);

            // 3. Construct the HTTP Request (The "Runner" Logic)
            // In a real app, the RestApiOperationRunner does this. We simulate it here.
            var finalUrl = $"{wrapper.BaseUrl}{wrapper.Path.Replace("{room}", room)}";
            var queryString = $"?temperature={temperature}";
            
            using var httpClient = new HttpClient();
            
            // 4. Apply Authentication
            var authHeader = wrapper.AuthProvider.GetAuthenticationHeader();
            httpClient.DefaultRequestHeaders.Authorization = authHeader;

            // 5. Execute the HTTP Call
            // We use POST for this example (simulating a state change).
            var content = new StringContent($"{{\"target\": {temperature}}}", Encoding.UTF8, "application/json");
            
            // NOTE: Since the URL is fake, we catch the exception to show the logic flow.
            try 
            {
                var response = await httpClient.PostAsync(finalUrl + queryString, content);
                return $"Success (Status: {response.StatusCode})";
            }
            catch (HttpRequestException)
            {
                return "Simulated Success (Network error expected with fake URL, but logic is correct)";
            }
        }
    }

    // --- Architecture Block: Supporting Classes ---

    // Represents the Basic Auth requirement of the legacy system
    public class BasicAuthenticationProvider
    {
        private readonly string _username;
        private readonly string _password;

        public BasicAuthenticationProvider(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public System.Net.Http.Headers.AuthenticationHeaderValue GetAuthenticationHeader()
        {
            var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            return new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);
        }
    }

    // Represents the Parameter Mapping concept
    public class RestApiParameter
    {
        public string Name { get; }
        public Type Type { get; }
        public string Description { get; }
        public RestApiParameterLocation Location { get; }
        public bool IsRequired { get; }

        public RestApiParameter(string name, Type type, string description, RestApiParameterLocation location, bool isRequired)
        {
            Name = name;
            Type = type;
            Description = description;
            Location = location;
            IsRequired = isRequired;
        }
    }

    public enum RestApiParameterLocation { Path, Query, Body }

    // The Wrapper Object that holds the Plugin Metadata
    public class ThermostatWrapper
    {
        public KernelFunction Function { get; }
        public string BaseUrl { get; }
        public string Path { get; }
        public BasicAuthenticationProvider AuthProvider { get; }
        public List<RestApiParameter> Parameters { get; }

        public ThermostatWrapper(KernelFunction function, string baseUrl, string path, BasicAuthenticationProvider authProvider, List<RestApiParameter> parameters)
        {
            Function = function;
            BaseUrl = baseUrl;
            Path = path;
            AuthProvider = authProvider;
            Parameters = parameters;
        }
    }
}

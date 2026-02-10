
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

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Yaml;

namespace Exercise5_VersioningRollback
{
    // Configuration structure
    public class AppConfig
    {
        public string ActiveVersion { get; set; } = "v2";
    }

    public class PromptRegistry
    {
        private readonly string _configPath;
        private readonly Kernel _kernel;

        public PromptRegistry(Kernel kernel, string configPath)
        {
            _kernel = kernel;
            _configPath = configPath;
        }

        public async Task<KernelFunction> LoadFunctionAsync()
        {
            var configJson = await File.ReadAllTextAsync(_configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(configJson);
            
            string activeVersion = config.ActiveVersion;
            string yamlPath = $"data_extraction_{activeVersion}.yaml";

            try
            {
                Console.WriteLine($"Attempting to load version: {activeVersion}");
                return await TryLoadVersion(yamlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load version {activeVersion}: {ex.Message}");
                
                // Rollback Logic
                if (activeVersion == "v2")
                {
                    Console.WriteLine("Initiating rollback to v1...");
                    return await TryLoadVersion("data_extraction_v1.yaml");
                }
                
                throw; // If v1 fails, we have no fallback.
            }
        }

        private async Task<KernelFunction> TryLoadVersion(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"YAML file not found: {path}");
            
            string yaml = await File.ReadAllTextAsync(path);
            // Simulating a failure for v2 if the file is corrupted or invalid
            // In a real scenario, deserialization might throw exceptions for schema mismatches
            return KernelPromptTemplateFactory.CreateFunctionFromYaml(yaml);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Create Mock YAML files
            // v1: Simple schema
            string v1Yaml = @"
name: DataExtractor
input_parameters:
  - name: text
    type: string
template: Extract dates and amounts from: {{text}}
";
            // v2: Includes new required parameter 'ExtractionMode'
            string v2Yaml = @"
name: DataExtractor
input_parameters:
  - name: text
    type: string
  - name: ExtractionMode
    type: string
    required: true
template: Extract dates and amounts in {{ExtractionMode}} mode from: {{text}}
";
            
            await File.WriteAllTextAsync("data_extraction_v1.yaml", v1Yaml);
            
            // 2. Simulate Corrupted v2 (e.g., missing required field in schema or invalid YAML)
            // We will write invalid YAML to trigger the catch block
            await File.WriteAllTextAsync("data_extraction_v2.yaml", "invalid: yaml: content: [");

            // Create config
            var config = new AppConfig { ActiveVersion = "v2" };
            await File.WriteAllTextAsync("appsettings.json", JsonSerializer.Serialize(config));

            // 3. Execution Flow
            var kernel = Kernel.CreateBuilder().Build();
            var registry = new PromptRegistry(kernel, "appsettings.json");

            try
            {
                var function = await registry.LoadFunctionAsync();
                
                // Attempt to invoke. 
                // Note: v1 doesn't require 'ExtractionMode', so this works.
                // If v2 was loaded (and valid), we would need to pass ExtractionMode.
                var result = await kernel.InvokeAsync(function, new KernelArguments 
                { 
                    ["text"] = "Invoice #123 dated 2023-10-01 for $500.00" 
                });
                
                Console.WriteLine($"Success! Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Error: {ex.Message}");
            }
        }
    }
}

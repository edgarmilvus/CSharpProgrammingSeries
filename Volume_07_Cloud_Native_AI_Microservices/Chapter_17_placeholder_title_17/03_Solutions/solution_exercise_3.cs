
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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;

// 1. Configuration Class with Source Generators support
public partial class GpuConfig
{
    [JsonPropertyName("gpuLimit")]
    public string GpuLimit { get; set; } // e.g., "1"

    [JsonPropertyName("models")]
    public List<ModelConfig> Models { get; set; } = new();
}

public class ModelConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("vramRequirementGiB")]
    public double VramRequirementGiB { get; set; }
}

// 2. Validation Logic
public class ConfigValidator
{
    // Assuming a physical limit (e.g., A100 has 40GB or 80GB). 
    // In a real app, this might be queried from the node via NVIDIA Management Library (NVML).
    private const double MaxPhysicalVramGiB = 40.0; 

    public ValidationResult Validate(GpuConfig config)
    {
        if (config.Models == null || !config.Models.Any())
            return ValidationResult.Success;

        // Calculate total requested VRAM
        var totalRequestedVram = config.Models.Sum(m => m.VramRequirementGiB);

        // Check against the limit defined in the deployment (parsed from string "1" -> 1 GPU)
        // Or check against physical capacity.
        // Here we check if the sum exceeds the physical capacity of the requested GPU count.
        int requestedGpus = int.Parse(config.GpuLimit);
        double availableVram = requestedGpus * MaxPhysicalVramGiB;

        if (totalRequestedVram > availableVram)
        {
            return new ValidationResult(
                $"Total VRAM required ({totalRequestedVram} GiB) exceeds available VRAM ({availableVram} GiB) for {requestedGpus} GPU(s).");
        }

        return ValidationResult.Success;
    }
}

// 3. Usage Example
public class Program
{
    public static void Main()
    {
        var json = """
        {
            "gpuLimit": "1",
            "models": [
                { "name": "SmallModel", "vramRequirementGiB": 5.0 },
                { "name": "MediumModel", "vramRequirementGiB": 12.0 }
            ]
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var config = JsonSerializer.Deserialize<GpuConfig>(json, options);
        
        var validator = new ConfigValidator();
        var result = validator.Validate(config);

        if (result != ValidationResult.Success)
            Console.WriteLine($"Validation Failed: {result.ErrorMessage}");
        else
            Console.WriteLine("Configuration Validated Successfully.");
    }
}

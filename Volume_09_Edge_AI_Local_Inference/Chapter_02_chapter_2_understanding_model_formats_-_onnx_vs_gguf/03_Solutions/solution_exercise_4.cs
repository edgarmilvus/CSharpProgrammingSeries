
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class ConversionException : Exception
{
    public string ErrorOutput { get; }
    public int ExitCode { get; }

    public ConversionException(int exitCode, string errorOutput) 
        : base($"Conversion failed with exit code {exitCode}. Error: {errorOutput}")
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }
}

public class ModelConverter
{
    public async Task<string> ConvertHuggingFaceToOnnx(string modelId, string outputDirectory)
    {
        // 1. Check if tool is available (Simulated)
        // In a real scenario, we might run `optimum-cli --version` first.
        Console.WriteLine($"[Check] Verifying 'optimum-cli' availability...");

        // 2. Prepare Output Directory
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // 3. Construct Process Start Info
        var startInfo = new ProcessStartInfo
        {
            FileName = "optimum-cli", // Assuming it's in PATH
            Arguments = $"export onnx --model {modelId} --task text-generation {outputDirectory}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine($"[Info] Starting conversion for '{modelId}'...");
        Console.WriteLine($"[Cmd] {startInfo.FileName} {startInfo.Arguments}");

        // 4. Execute Process
        using var process = new Process { StartInfo = startInfo };
        
        // Capture output for logging
        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, args) => 
        {
            if (args.Data != null) 
            {
                Console.WriteLine($"[LOG] {args.Data}");
                outputBuilder.AppendLine(args.Data);
            }
        };
        
        process.ErrorDataReceived += (sender, args) => 
        {
            if (args.Data != null) 
            {
                Console.WriteLine($"[ERR] {args.Data}");
                errorBuilder.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 5. Wait for completion
        await process.WaitForExitAsync();

        // 6. Error Handling
        if (process.ExitCode != 0)
        {
            throw new ConversionException(process.ExitCode, errorBuilder.ToString());
        }

        // 7. Post-Conversion Validation
        string modelPath = Path.Combine(outputDirectory, "model.onnx");
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException("Conversion appeared successful, but 'model.onnx' was not found.");
        }

        var fileInfo = new FileInfo(modelPath);
        if (fileInfo.Length == 0)
        {
            throw new InvalidDataException("Generated 'model.onnx' file is empty.");
        }

        Console.WriteLine($"[Success] Model converted to: {modelPath} ({fileInfo.Length / 1024.0:F2} KB)");
        return modelPath;
    }
}

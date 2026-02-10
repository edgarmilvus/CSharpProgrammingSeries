
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

// C# Sidecar: DockerBuilder.cs
// This application executes the Docker build process, capturing output to parse specific build stages.
// Prerequisites: Docker must be installed and accessible in the environment path.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class DockerBuilder
{
    public static async Task Main(string[] args)
    {
        string dockerfilePath = "./Dockerfile";
        string buildContext = ".";
        string imageName = "ai-agent:optimized";

        if (!File.Exists(dockerfilePath))
        {
            Console.WriteLine($"Error: Dockerfile not found at {dockerfilePath}");
            return;
        }

        Console.WriteLine($"Starting Docker build for image: {imageName}...");
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"build -t {imageName} -f {dockerfilePath} {buildContext}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        
        // Use StringBuilder to buffer lines for stage detection
        var outputBuilder = new StringBuilder();
        
        // Event handler for real-time output parsing
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                string line = e.Data;
                Console.WriteLine(line); // Pass-through raw output

                // Detect specific build stages based on Docker build output conventions
                if (line.Contains("Installing dependencies", StringComparison.OrdinalIgnoreCase) || 
                    line.Contains("RUN pip install", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[C# Sidecar] DETECTED STAGE: Dependency Installation");
                }
                else if (line.Contains("Copying files", StringComparison.OrdinalIgnoreCase) || 
                         line.Contains("COPY", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[C# Sidecar] DETECTED STAGE: File Copying");
                }
                else if (line.Contains("Successfully built", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[C# Sidecar] DETECTED STAGE: Build Complete");
                }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null) Console.WriteLine($"ERROR: {e.Data}");
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                Console.WriteLine("\n[C# Sidecar] Docker build completed successfully.");
            }
            else
            {
                Console.WriteLine($"\n[C# Sidecar] Docker build failed with exit code {process.ExitCode}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[C# Sidecar] Exception during execution: {ex.Message}");
        }
    }
}

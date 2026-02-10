
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
using System.Text;

// 1. Strict Lifecycle Enum
public enum PipelineLifecycle
{
    Uninitialized,
    Ready,
    Running,
    Faulted
}

// 2. Composite Status Flags
[Flags]
public enum PipelineStatus
{
    None = 0,
    Buffering = 1,
    AwaitingGPU = 2,
    Processing = 4,
    Error = 8
}

public class AdvancedTensorPipeline
{
    private PipelineLifecycle _lifecycle = PipelineLifecycle.Uninitialized;
    private PipelineStatus _status = PipelineStatus.None;

    public void Initialize()
    {
        // Guard clause: Cannot initialize if already faulted
        if (_lifecycle == PipelineLifecycle.Faulted)
        {
            throw new InvalidOperationException("System is faulted. Cannot initialize.");
        }

        _lifecycle = PipelineLifecycle.Ready;
        _status = PipelineStatus.None; // Reset all flags
        Console.WriteLine("Pipeline Initialized. Lifecycle: Ready");
    }

    public void StartProcessing()
    {
        // Guard clause: Only Ready state can start processing
        if (_lifecycle != PipelineLifecycle.Ready)
        {
            throw new InvalidOperationException($"Cannot start processing from lifecycle state: {_lifecycle}");
        }

        _lifecycle = PipelineLifecycle.Running;
        
        // Set composite flags using bitwise OR
        _status = PipelineStatus.Processing | PipelineStatus.Buffering;
        
        Console.WriteLine("Processing Started. Lifecycle: Running");
    }

    public void SimulateError()
    {
        // Update strict state
        _lifecycle = PipelineLifecycle.Faulted;
        
        // Add Error flag to existing flags using bitwise OR
        _status |= PipelineStatus.Error;
        
        Console.WriteLine("CRITICAL ERROR SIMULATED.");
    }

    public string ReportStatus()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Lifecycle: {_lifecycle}");
        sb.Append(" | Active Flags: ");

        if (_status == PipelineStatus.None)
        {
            sb.Append("None");
        }
        else
        {
            // Manual string construction for flags (No LINQ)
            bool first = true;
            
            // Check each flag individually
            if ((_status & PipelineStatus.Buffering) == PipelineStatus.Buffering)
            {
                if (!first) sb.Append(", ");
                sb.Append("Buffering");
                first = false;
            }
            if ((_status & PipelineStatus.AwaitingGPU) == PipelineStatus.AwaitingGPU)
            {
                if (!first) sb.Append(", ");
                sb.Append("AwaitingGPU");
                first = false;
            }
            if ((_status & PipelineStatus.Processing) == PipelineStatus.Processing)
            {
                if (!first) sb.Append(", ");
                sb.Append("Processing");
                first = false;
            }
            if ((_status & PipelineStatus.Error) == PipelineStatus.Error)
            {
                if (!first) sb.Append(", ");
                sb.Append("Error");
                first = false;
            }
        }

        return sb.ToString();
    }
}

// Test Harness
public class Program
{
    public static void Main()
    {
        AdvancedTensorPipeline pipeline = new AdvancedTensorPipeline();

        try
        {
            Console.WriteLine(pipeline.ReportStatus()); // Uninitialized

            pipeline.Initialize(); // -> Ready
            Console.WriteLine(pipeline.ReportStatus());

            pipeline.StartProcessing(); // -> Running + Buffering/Processing
            Console.WriteLine(pipeline.ReportStatus());

            pipeline.SimulateError(); // -> Faulted + Error
            Console.WriteLine(pipeline.ReportStatus());

            // This should throw an exception
            pipeline.StartProcessing(); 
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}

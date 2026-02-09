
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

// 1. C# Model mirroring the CRD Spec
public class InferencePipelineSpec
{
    [JsonPropertyName("steps")]
    public List<PipelineStep> Steps { get; set; } = new();

    [JsonPropertyName("parallelism")]
    public int Parallelism { get; set; } = 1;

    [JsonPropertyName("inputTopic")]
    public string InputTopic { get; set; } = string.Empty;

    [JsonPropertyName("outputTopic")]
    public string OutputTopic { get; set; } = string.Empty;
}

public class PipelineStep
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    // Interactive Challenge: Dependency Resolution
    [JsonPropertyName("dependsOn")]
    public List<string> DependsOn { get; set; } = new();
}

// Simulated Enums and State Store
public enum JobStatus { Pending, Running, Succeeded, Failed }

public class InferencePipelineController
{
    // Simulated Kubernetes Job Store
    private readonly Dictionary<string, JobStatus> _jobStore = new();

    // Reconciliation Loop
    public void Reconcile(InferencePipelineSpec pipeline)
    {
        Console.WriteLine("--- Starting Reconciliation Loop ---");

        // 1. Parse and Validate
        if (pipeline.Steps == null || !pipeline.Steps.Any())
        {
            Console.WriteLine("No steps defined in pipeline.");
            return;
        }

        // 2. Handle Dependency Resolution
        foreach (var step in pipeline.Steps)
        {
            // Check if job already exists
            if (_jobStore.ContainsKey(step.Name))
            {
                // Simulate status update (randomly progress status for demo)
                UpdateJobStatus(step.Name);
                continue; 
            }

            // Check Dependencies
            if (step.DependsOn != null && step.DependsOn.Any())
            {
                bool dependenciesMet = true;
                foreach (var dep in step.DependsOn)
                {
                    if (!_jobStore.ContainsKey(dep) || _jobStore[dep] != JobStatus.Succeeded)
                    {
                        dependenciesMet = false;
                        Console.WriteLine($"Step '{step.Name}' waiting for dependency '{dep}'. Current status: {_jobStore.GetValueOrDefault(dep, JobStatus.Pending)}");
                        break;
                    }
                }

                if (!dependenciesMet) continue;
            }

            // 3. Create Job
            CreateJob(step, pipeline.Parallelism);
        }

        Console.WriteLine("--- Reconciliation Complete ---");
    }

    private void CreateJob(PipelineStep step, int parallelism)
    {
        Console.WriteLine($"[Action] Creating Job for Step: '{step.Name}' (Image: {step.Image}, Parallelism: {parallelism})");
        
        // Simulate Job Creation
        _jobStore[step.Name] = JobStatus.Pending;
        
        // Simulate immediate start
        _jobStore[step.Name] = JobStatus.Running;
    }

    private void UpdateJobStatus(string stepName)
    {
        // Simulation logic: Randomly complete jobs to allow downstream steps to run
        var rnd = new Random();
        if (rnd.Next(10) > 7) // 30% chance to finish
        {
            _jobStore[stepName] = JobStatus.Succeeded;
            Console.WriteLine($"[Status Update] Job '{stepName}' Succeeded.");
        }
    }

    public void SimulateScaling(InferencePipelineSpec pipeline, int newParallelism)
    {
        Console.WriteLine($"[Action] Scaling Pipeline Parallelism to {newParallelism}");
        pipeline.Parallelism = newParallelism;
        // In a real controller, this would patch the Deployment/Job specs
        foreach(var step in pipeline.Steps)
        {
             Console.WriteLine($"  - Scaling Job '{step.Name}' to {newParallelism} replicas.");
        }
    }

    public void SimulateDeletion(InferencePipelineSpec pipeline)
    {
        Console.WriteLine("[Action] Deleting Pipeline Resources");
        foreach (var step in pipeline.Steps)
        {
            if (_jobStore.ContainsKey(step.Name))
            {
                _jobStore.Remove(step.Name);
                Console.WriteLine($"  - Deleted Job for Step: '{step.Name}'");
            }
        }
    }
}

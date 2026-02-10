
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

// 1. Protocol Definition (Inference.proto)
// syntax = "proto3";
// option csharp_namespace = "AiAgent.Grpc";
// 
// service InferenceRouter {
//   rpc RouteRequest (InferenceRequest) returns (InferenceResponse);
// }
//
// message InferenceRequest {
//   string text = 1;
//   string task_type = 2; // e.g., "summarize", "extract"
// }
//
// message InferenceResponse {
//   string result = 1;
//   string processed_by = 2;
// }

// 2. Router Implementation (RouterService.cs)
using Grpc.Core;
using AiAgent.Grpc; // Generated from .proto
using System.Collections.Generic;
using System.Threading.Tasks;

public class RouterService : InferenceRouter.InferenceRouterBase
{
    // Simple mapping of task types to target worker addresses (in a real app, use service discovery)
    private readonly Dictionary<string, string> _workerMap = new Dictionary<string, string>
    {
        { "summarize", "http://summarization-worker:5000" },
        { "extract", "http://extraction-worker:5000" }
    };

    public override async Task<InferenceResponse> RouteRequest(InferenceRequest request, ServerCallContext context)
    {
        string targetWorker = "unknown";
        
        // Inspect metadata and route
        if (_workerMap.ContainsKey(request.TaskType))
        {
            targetWorker = _workerMap[request.TaskType];
        }

        // Simulate forwarding to the worker (In a real scenario, create a gRPC client here)
        // For this exercise, we simulate the response.
        return new InferenceResponse
        {
            Result = $"Processed '{request.Text}' for task '{request.TaskType}'",
            ProcessedBy = $"Router->Worker at {targetWorker}"
        };
    }
}

// 3. Worker Implementation (WorkerService.cs)
using Grpc.Core;
using AiAgent.Grpc;
using System.Threading.Tasks;

public class WorkerService : InferenceRouter.InferenceRouterBase
{
    private readonly string _workerName;

    public WorkerService(string workerName)
    {
        _workerName = workerName;
    }

    public override async Task<InferenceResponse> RouteRequest(InferenceRequest request, ServerCallContext context)
    {
        // Simulate AI processing latency
        await Task.Delay(100); // Represents model inference time

        return new InferenceResponse
        {
            Result = $"[Simulated Model Output] Summary of: {request.Text}",
            ProcessedBy = _workerName
        };
    }
}

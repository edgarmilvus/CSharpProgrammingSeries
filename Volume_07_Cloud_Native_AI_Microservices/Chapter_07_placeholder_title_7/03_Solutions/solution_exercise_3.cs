
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

public class OrchestratorService
{
    private readonly DaprClient _daprClient; // Assuming Dapr Pub/Sub
    private readonly ILogger<OrchestratorService> _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<AggregatedResult>> _pendingTasks;

    public OrchestratorService(DaprClient daprClient, ILogger<OrchestratorService> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
        _pendingTasks = new ConcurrentDictionary<string, TaskCompletionSource<AggregatedResult>>();
    }

    public async Task<AggregatedResult> ProcessDocumentAsync(string document)
    {
        var taskId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<AggregatedResult>();
        
        // Store the TCS to be completed when results arrive
        _pendingTasks.TryAdd(taskId, tcs);

        // 1. Define the task and required workers
        var task = new DocumentTask(taskId, document, new List<string> { "Summarizer", "SentimentAnalyzer" });
        
        // 2. Publish the task event to the 'tasks' topic
        await _daprClient.PublishEventAsync("pubsub", "tasks", task);

        // 3. Set a timeout using CancellationToken
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        cts.Token.Register(() => tcs.TrySetCanceled());

        try
        {
            // Await the aggregated result (completed by the background listener)
            return await tcs.Task;
        }
        finally
        {
            _pendingTasks.TryRemove(taskId, out _);
        }
    }

    // Background method listening for 'results' topic
    // In ASP.NET Core, this would be a Dapr Topic Subscription
    [Topic("pubsub", "results")]
    public async Task HandleResult(ProcessingResult result)
    {
        if (!_pendingTasks.TryGetValue(result.TaskId, out var tcs)) return;

        // Logic to aggregate results (simplified)
        // In reality, you'd track which workers have replied and wait for all expected types
        // For this example, we assume the last result triggers completion or we use a state store to aggregate
        
        // Let's simulate aggregation logic:
        // Ideally, we would use Dapr State Store to store partial results for the TaskId
        // and check if all expected workers have reported.
        
        // If all workers done:
        // var aggregated = new AggregatedResult(result.TaskId, ...);
        // tcs.TrySetResult(aggregated);
    }
}

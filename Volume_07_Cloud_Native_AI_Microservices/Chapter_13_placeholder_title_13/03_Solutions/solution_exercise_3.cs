
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

// Program.cs
using System.Threading.Channels;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddSingleton<Channel<AnalysisJob>>(_ => 
    Channel.CreateUnbounded<AnalysisJob>());
builder.Services.AddSingleton<IJobTracker, InMemoryJobTracker>();
builder.Services.AddHostedService<AnalysisWorker>();

var app = builder.Build();

// 1. Producer Service
app.MapPost("/analyze", async (string imageUrl, Channel<AnalysisJob> channel, IJobTracker tracker) =>
{
    var jobId = Guid.NewGuid();
    var job = new AnalysisJob(JobId: jobId, ImageUrl: imageUrl);

    // Push to queue
    await channel.Writer.WriteAsync(job);
    
    // Track status
    tracker.UpdateStatus(jobId, JobStatus.Pending);

    // Return 202 Accepted with JobId
    return Results.Accepted($"/jobs/{jobId}", new { JobId = jobId });
});

// 5. Polling Endpoint
app.MapGet("/jobs/{jobId}", (Guid jobId, IJobTracker tracker) =>
{
    var status = tracker.GetStatus(jobId);
    return status is not null ? Results.Ok(status) : Results.NotFound();
});

app.Run();

// ---------------------------------------------------------
// Data Models
public record AnalysisJob(Guid JobId, string ImageUrl);
public enum JobStatus { Pending, Processing, Completed, Failed }
public record JobStatusDto(Guid JobId, JobStatus Status, string? ErrorMessage = null);

// ---------------------------------------------------------
// 4. State Tracking Interface & Implementation
public interface IJobTracker
{
    void UpdateStatus(Guid jobId, JobStatus status, string? error = null);
    JobStatusDto? GetStatus(Guid jobId);
}

public class InMemoryJobTracker : IJobTracker
{
    private readonly ConcurrentDictionary<Guid, JobStatusDto> _store = new();

    public void UpdateStatus(Guid jobId, JobStatus status, string? error = null)
    {
        _store[jobId] = new JobStatusDto(jobId, status, error);
    }

    public JobStatusDto? GetStatus(Guid jobId)
    {
        _store.TryGetValue(jobId, out var status);
        return status;
    }
}

// ---------------------------------------------------------
// 2. Consumer Service (Background Worker)
public class AnalysisWorker : BackgroundService
{
    private readonly Channel<AnalysisJob> _channel;
    private readonly IJobTracker _tracker;
    private readonly ILogger<AnalysisWorker> _logger;

    public AnalysisWorker(Channel<AnalysisJob> channel, IJobTracker tracker, ILogger<AnalysisWorker> logger)
    {
        _channel = channel;
        _tracker = tracker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            // Process job
            await ProcessJobAsync(job);
        }
    }

    private async Task ProcessJobAsync(AnalysisJob job)
    {
        _tracker.UpdateStatus(job.JobId, JobStatus.Processing);

        try
        {
            // 3. Simulate long-running AI inference
            _logger.LogInformation("Processing Job {JobId} for Image {Url}", job.JobId, job.ImageUrl);

            // Interactive Challenge: Poison Message Handling
            if (job.ImageUrl.Contains("crash"))
            {
                // Simulate failure logic
                throw new InvalidOperationException("Simulated crash due to invalid image format.");
            }

            await Task.Delay(2000); // Simulate processing time

            _tracker.UpdateStatus(job.JobId, JobStatus.Completed);
            _logger.LogInformation("Job {JobId} Completed.", job.JobId);
        }
        catch (Exception ex)
        {
            // Interactive Challenge: Retry Logic & Dead Letter Queue
            // In a real scenario, we would check a retry count stored in a wrapper object.
            // For this simulation, we assume the wrapper logic exists or we log to a "Dead Letter" collection.
            
            _logger.LogError(ex, "Job {JobId} failed. Moving to Dead Letter Queue.", job.JobId);
            _tracker.UpdateStatus(job.JobId, JobStatus.Failed, ex.Message);
        }
    }
}

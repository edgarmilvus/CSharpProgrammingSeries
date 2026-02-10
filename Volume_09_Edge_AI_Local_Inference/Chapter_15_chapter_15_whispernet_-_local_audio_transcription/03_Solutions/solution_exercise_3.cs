
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record BatchOptions(
    int MaxConcurrentTranscriptions = 4,
    int MaxMemoryMB = 2048,
    string OutputDirectory = "output",
    bool OverwriteExisting = false
);

public record BatchProgress(
    int TotalFiles,
    int CompletedFiles,
    double CurrentMemoryUsageMB,
    ConcurrentDictionary<string, double> FileProgress,
    TimeSpan EstimatedRemaining
);

public class BatchTranscriptionEngine
{
    private readonly BatchOptions _options;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<TranscriptionResult>> _taskRegistry;

    public BatchTranscriptionEngine(BatchOptions options)
    {
        _options = options;
        // Limit concurrency based on options
        _semaphore = new SemaphoreSlim(options.MaxConcurrentTranscriptions);
        _taskRegistry = new ConcurrentDictionary<string, TaskCompletionSource<TranscriptionResult>>();
        
        // Ensure output directory exists
        Directory.CreateDirectory(options.OutputDirectory);
    }

    public async Task<BatchResult> ProcessBatchAsync(
        IEnumerable<string> audioFiles,
        IProgress<BatchProgress> progress = null,
        CancellationToken cancellationToken = default)
    {
        var files = audioFiles.ToList();
        var totalFiles = files.Count;
        var completedCount = 0;
        var results = new ConcurrentBag<TranscriptionResult>();
        var startTime = DateTime.Now;

        // Create tasks for all files
        var tasks = files.Select(async file =>
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Resource Check before execution
                await CheckResourcesAsync(cancellationToken);

                // Execute Transcription
                var result = await TranscribeSingleFileAsync(file, cancellationToken);
                results.Add(result);

                // Update Progress
                Interlocked.Increment(ref completedCount);
                ReportProgress(progress, totalFiles, completedCount, startTime);
                
                return result;
            }
            finally
            {
                _semaphore.Release();
            }
        }).ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            // Handle batch failure or individual failures
            // In a real system, we might aggregate errors instead of throwing immediately
            Console.WriteLine($"Batch processing encountered errors: {ex.Message}");
        }

        return new BatchResult { SuccessfulResults = results.ToList() };
    }

    private async Task CheckResourcesAsync(CancellationToken ct)
    {
        // Monitor Memory
        var process = Process.GetCurrentProcess();
        var memoryMB = process.WorkingSet64 / (1024 * 1024);

        if (memoryMB > _options.MaxMemoryMB)
        {
            // Force GC and wait
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Delay(1000, ct); // Give OS time to reclaim memory

            // Re-check
            process.Refresh();
            memoryMB = process.WorkingSet64 / (1024 * 1024);
            
            if (memoryMB > _options.MaxMemoryMB)
                throw new InvalidOperationException($"Memory limit exceeded: {memoryMB}MB > {_options.MaxMemoryMB}MB");
        }
    }

    private async Task<TranscriptionResult> TranscribeSingleFileAsync(string file, CancellationToken ct)
    {
        // Simulate transcription logic
        // In reality, instantiate AudioTranscriber here or use a pooled model
        await Task.Delay(new Random().Next(500, 2000), ct); // Simulate processing time
        
        // Save output if configured
        var outputFileName = Path.Combine(_options.OutputDirectory, Path.GetFileNameWithoutExtension(file) + ".txt");
        if (!_options.OverwriteExisting && File.Exists(outputFileName))
            return new TranscriptionResult($"Skipped: {file}", null, null);

        await File.WriteAllTextAsync(outputFileName, "Transcription text...", ct);

        return new TranscriptionResult($"Success: {file}", null, null);
    }

    private void ReportProgress(IProgress<BatchProgress> progress, int total, int completed, DateTime start)
    {
        if (progress == null) return;

        var elapsed = DateTime.Now - start;
        var avgTimePerFile = elapsed.TotalSeconds / Math.Max(1, completed);
        var remainingFiles = total - completed;
        var estimatedRemaining = TimeSpan.FromSeconds(avgTimePerFile * remainingFiles);

        var memInfo = GC.GetGCMemoryInfo();
        var memMB = memInfo.HeapSizeBytes / (1024 * 1024);

        progress.Report(new BatchProgress(
            total,
            completed,
            memMB,
            new ConcurrentDictionary<string, double>(), // Simplified
            estimatedRemaining
        ));
    }
}

public class BatchResult 
{
    public List<TranscriptionResult> SuccessfulResults { get; set; } = new();
}

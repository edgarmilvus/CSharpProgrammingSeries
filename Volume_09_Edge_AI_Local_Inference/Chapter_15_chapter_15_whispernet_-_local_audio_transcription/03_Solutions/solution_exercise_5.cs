
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whisper.net.Ggml;

public enum QuantizationLevel { FP32, FP16, INT8 }
public enum ExecutionProvider { CPU, CUDA, TensorRT }

public record ModelConfig(
    string ModelId,
    GgmlType Size,
    QuantizationLevel Quantization = QuantizationLevel.FP32,
    ExecutionProvider Provider = ExecutionProvider.CPU
);

public record ModelInfo(string Id, string Version, long SizeOnDisk, bool IsQuantized);

public class ModelRepository : IModelRepository
{
    private readonly string _storagePath;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, IWhisperModel> _loadedModels = new(); // LRU Cache simulation

    public ModelRepository(string storagePath)
    {
        _storagePath = storagePath;
        Directory.CreateDirectory(_storagePath);
        _httpClient = new HttpClient();
    }

    public async Task<IWhisperModel> GetModelAsync(ModelConfig config, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{config.ModelId}_{config.Size}_{config.Quantization}_{config.Provider}";
        
        // Check Cache
        if (_loadedModels.TryGetValue(cacheKey, out var cachedModel))
            return cachedModel;

        // 1. Download if missing
        var modelPath = Path.Combine(_storagePath, $"{config.ModelId}.bin");
        if (!File.Exists(modelPath))
        {
            await DownloadModelAsync(config.ModelId, modelPath, cancellationToken);
        }

        // 2. Quantize if necessary (and if not already quantized)
        if (config.Quantization != QuantizationLevel.FP32)
        {
            // Check if quantized version exists
            var quantizedPath = Path.Combine(_storagePath, $"{config.ModelId}_{config.Quantization}.bin");
            if (!File.Exists(quantizedPath))
            {
                await QuantizeModelAsync(config.ModelId, config.Quantization, cancellationToken);
            }
            modelPath = quantizedPath; // Use quantized version
        }

        // 3. Create Model Wrapper (Simulating Whisper.net model loading)
        var model = new WhisperModelWrapper(modelPath, config);
        
        // Add to cache
        _loadedModels[cacheKey] = model;
        
        return model;
    }

    public async Task<QuantizationReport> QuantizeModelAsync(string modelId, QuantizationLevel level, CancellationToken cancellationToken = default)
    {
        // In a real scenario, this would call ONNX Runtime tools or a separate quantization library.
        // For this exercise, we simulate the process and create a dummy file.
        
        var inputPath = Path.Combine(_storagePath, $"{modelId}.bin");
        var outputPath = Path.Combine(_storagePath, $"{modelId}_{level}.bin");

        if (!File.Exists(inputPath)) throw new FileNotFoundException("Base model not found for quantization.");

        // Simulate heavy computation
        await Task.Delay(2000, cancellationToken); 
        
        // Simulate file creation (Copy for demo)
        File.Copy(inputPath, outputPath, true);

        return new QuantizationReport 
        { 
            ModelId = modelId, 
            OriginalSize = new FileInfo(inputPath).Length, 
            QuantizedSize = new FileInfo(outputPath).Length,
            Level = level
        };
    }

    public async Task<BenchmarkResult> BenchmarkModelAsync(string modelId, string audioSample, CancellationToken cancellationToken = default)
    {
        // Load model, process audio, measure time and memory
        var config = new ModelConfig(modelId, GgmlType.Base);
        var model = await GetModelAsync(config, cancellationToken);
        
        var sw = Stopwatch.StartNew();
        var startMem = GC.GetTotalAllocatedBytes(true);
        
        // Simulate transcription
        await model.TranscribeAsync(new byte[1024], new TranscriptionOptions(), cancellationToken);
        
        sw.Stop();
        var endMem = GC.GetTotalAllocatedBytes(true);

        return new BenchmarkResult
        {
            ModelId = modelId,
            Elapsed = sw.Elapsed,
            MemoryAllocated = endMem - startMem
        };
    }

    private async Task DownloadModelAsync(string modelId, string path, CancellationToken ct)
    {
        // Example URL structure (Hugging Face)
        var url = $"https://huggingface.co/ggerganov/whisper.cpp/resolve/main/{modelId}.bin";
        
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fs, ct);
    }

    public Task UpdateModelAsync(string modelId, string version, CancellationToken cancellationToken = default)
    {
        // Logic to check remote version vs local version and download if newer
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ModelInfo>> ListAvailableModels()
    {
        var files = Directory.GetFiles(_storagePath, "*.bin")
            .Select(f => new FileInfo(f))
            .Select(fi => new ModelInfo(fi.Name, "1.0", fi.Length, fi.Name.Contains("INT8")));
        
        return Task.FromResult(files.AsEnumerable());
    }

    public ValueTask DisposeAsync()
    {
        foreach (var model in _loadedModels.Values)
        {
            model.Dispose();
        }
        return ValueTask.CompletedTask;
    }
}

// Fluent API Builder
public static class ModelRepositoryExtensions
{
    public static async Task<IWhisperModel> GetModel(this Task<ModelRepository> repoTask, ModelConfig config)
    {
        var repo = await repoTask;
        return await repo.GetModelAsync(config);
    }
}

// Placeholder implementations
public class WhisperModelWrapper : IWhisperModel
{
    public WhisperModelWrapper(string path, ModelConfig config) { Info = new ModelInfo(config.ModelId, "1.0", 0, config.Quantization != QuantizationLevel.FP32); }
    public ModelInfo Info { get; }
    public bool IsQuantized => Info.IsQuantized;
    public Task<TranscriptionResult> TranscribeAsync(byte[] audio, TranscriptionOptions options, CancellationToken cancellationToken = default) 
        => Task.FromResult(new TranscriptionResult("Simulated", null, null));
    public void Dispose() { }
}

public interface IModelRepository : IAsyncDisposable
{
    Task<IWhisperModel> GetModelAsync(ModelConfig config, CancellationToken cancellationToken = default);
    Task UpdateModelAsync(string modelId, string version, CancellationToken cancellationToken = default);
    Task<IEnumerable<ModelInfo>> ListAvailableModels();
    Task<QuantizationReport> QuantizeModelAsync(string modelId, QuantizationLevel level, CancellationToken cancellationToken = default);
    Task<BenchmarkResult> BenchmarkModelAsync(string modelId, string audioSample, CancellationToken cancellationToken = default);
}

public interface IWhisperModel : IDisposable
{
    Task<TranscriptionResult> TranscribeAsync(byte[] audio, TranscriptionOptions options, CancellationToken cancellationToken = default);
    ModelInfo Info { get; }
    bool IsQuantized { get; }
}

public record QuantizationReport(string ModelId, long OriginalSize, long QuantizedSize, QuantizationLevel Level);
public record BenchmarkResult(string ModelId, TimeSpan Elapsed, long MemoryAllocated);
public record TranscriptionResult(string FullText, List<Segment> Segments, Dictionary<string, double> WordTimings);
public record Segment(TimeSpan Start, TimeSpan End, string Text, double Confidence);
public record TranscriptionOptions();

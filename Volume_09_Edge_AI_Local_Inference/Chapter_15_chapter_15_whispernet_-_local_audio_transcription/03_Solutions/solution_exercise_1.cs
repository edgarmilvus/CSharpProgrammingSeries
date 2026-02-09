
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using NAudio.Wave; // Assuming NAudio for audio processing

// Domain models
public record TranscriptionResult(string FullText, List<Segment> Segments, Dictionary<string, double> WordTimings);
public record Segment(TimeSpan Start, TimeSpan End, string Text, double Confidence);
public record AudioTranscriberOptions(
    GgmlType ModelSize = GgmlType.Base,
    string Language = "en",
    bool EnableVad = true,
    int MaxMemoryMB = 2048,
    int ProcessingChunkSize = 30 // seconds
);

public class AudioTranscriber : IDisposable
{
    private readonly AudioTranscriberOptions _options;
    private WhisperProcessor? _processor;
    private bool _disposed;

    public AudioTranscriber(AudioTranscriberOptions options)
    {
        _options = options;
    }

    public async Task<TranscriptionResult> TranscribeAsync(string audioFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(audioFilePath))
            throw new ArgumentException("File path cannot be empty.", nameof(audioFilePath));

        // 1. Validate
        ValidateAudioFile(audioFilePath);

        // 2. Preprocess (convert to 16kHz mono WAV)
        var tempFile = Path.GetTempFileName();
        try
        {
            await PreprocessAudio(audioFilePath, tempFile);

            // 3. Load Model (Lazy loading)
            await EnsureModelLoadedAsync(cancellationToken);

            // 4. Transcribe with Retry Logic
            return await TranscribeWithRetryAsync(tempFile, cancellationToken);
        }
        finally
        {
            // Cleanup temp file
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { /* Ignore cleanup errors */ }
            }
        }
    }

    public void ValidateAudioFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        // Check file size (basic corruption check)
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length == 0)
            throw new InvalidDataException("Audio file is empty.");

        // Check duration (using NAudio)
        try
        {
            using (var reader = new AudioFileReader(filePath))
            {
                var duration = reader.TotalTime;
                if (duration.TotalMinutes > 10)
                    throw new InvalidDataException("Audio file exceeds maximum duration of 10 minutes.");
            }
        }
        catch (Exception ex) when (ex is not InvalidDataException)
        {
            throw new InvalidDataException("Audio file is corrupted or unsupported format.", ex);
        }
    }

    public async Task PreprocessAudio(string inputPath, string outputPath)
    {
        // In a real scenario, use FFmpeg CLI or a library like FFmpeg.AutoGen.
        // Here we use NAudio to convert to WAV 16kHz Mono as a simulation.
        try
        {
            using (var reader = new AudioFileReader(inputPath))
            {
                var outFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16-bit, Mono
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    resampler.ResamplerQuality = 60; // Highest quality
                    WaveFileWriter.CreateWaveFile(outputPath, resampler);
                }
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to preprocess audio. Ensure FFmpeg or NAudio supports the input format.", ex);
        }
    }

    private async Task EnsureModelLoadedAsync(CancellationToken cancellationToken)
    {
        if (_processor != null) return;

        // Check memory constraint
        var memoryInfo = GC.GetGCMemoryInfo();
        if (memoryInfo.HeapSizeBytes > _options.MaxMemoryMB * 1024 * 1024)
            throw new InsufficientMemoryException("System memory is insufficient for model loading.");

        // Load model based on options
        var builder = WhisperFactory.FromPath(GetModelPath(_options.ModelSize))
            .WithLanguage(_options.Language);

        if (_options.EnableVad)
            builder.WithSegmentProcessor(_ => { }); // Placeholder for VAD setup

        _processor = builder.Build();
    }

    private string GetModelPath(GgmlType size) => Path.Combine("models", $"ggml-{size}.bin"); // Simplified path

    private async Task<TranscriptionResult> TranscribeWithRetryAsync(string audioPath, CancellationToken cancellationToken)
    {
        int retries = 0;
        TimeSpan delay = TimeSpan.FromSeconds(1);

        while (true)
        {
            try
            {
                return await ProcessAudioStreamAsync(audioPath, cancellationToken);
            }
            catch (OutOfMemoryException)
            {
                // Chunked processing fallback for large files
                return await ProcessAudioInChunksAsync(audioPath, cancellationToken);
            }
            catch (Exception ex) when (retries < 3 && IsTransientError(ex))
            {
                retries++;
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(Math.Pow(2, retries)); // Exponential backoff
            }
        }
    }

    private async Task<TranscriptionResult> ProcessAudioStreamAsync(string audioPath, CancellationToken cancellationToken)
    {
        var segments = new List<Segment>();
        var fullText = new System.Text.StringBuilder();
        var wordTimings = new Dictionary<string, double>();

        // Use IAsyncEnumerable for streaming
        await foreach (var segment in RunWhisperAsync(audioPath, cancellationToken))
        {
            segments.Add(segment);
            fullText.AppendLine(segment.Text);
        }

        return new TranscriptionResult(fullText.ToString(), segments, wordTimings);
    }

    private async Task<TranscriptionResult> ProcessAudioInChunksAsync(string audioPath, CancellationToken cancellationToken)
    {
        // Implementation of chunking logic (simplified)
        // Read audio in 30-second chunks, process individually, merge results
        // This requires sophisticated audio slicing which is abstracted here.
        throw new NotImplementedException("Chunked processing requires audio slicing logic.");
    }

    private async IAsyncEnumerable<Segment> RunWhisperAsync(string audioPath, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Simulate streaming from Whisper.net
        // In real implementation, use WhisperFactory and iterate over segments
        using var fileStream = File.OpenRead(audioPath);
        
        // Note: Whisper.net typically processes the whole stream. 
        // For true chunking, we would slice the stream here.
        var segments = new List<Segment>(); // Placeholder for actual Whisper.net output
        await Task.CompletedTask; // Simulate async work
        
        yield return new Segment(TimeSpan.Zero, TimeSpan.FromSeconds(2.5), "Simulated transcription text", 0.95);
    }

    private bool IsTransientError(Exception ex)
    {
        return ex is IOException || 
               ex is System.Net.Sockets.SocketException || 
               ex is TimeoutException;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _processor?.Dispose();
            _disposed = true;
        }
    }
}

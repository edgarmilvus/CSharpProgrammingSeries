
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;

// --- 1. Configuration Models ---

public class VoiceProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelPath { get; set; } = string.Empty;
}

public class VoiceConfig
{
    public List<VoiceProfile> Profiles { get; set; } = new();
}

// --- 2. Thread-Safe Model Cache ---

// Requirement: Singleton, Thread-Safe, Disposable
public class ModelCache : IDisposable
{
    private readonly ILogger<ModelCache> _logger;
    // Key: SpeakerId, Value: Lazy<InferenceSession> ensures thread-safe initialization
    private readonly ConcurrentDictionary<int, Lazy<InferenceSession>> _sessions = new();

    public ModelCache(ILogger<ModelCache> logger)
    {
        _logger = logger;
    }

    public InferenceSession GetSession(int speakerId, string modelPath)
    {
        // Lazy<T> ensures the factory runs only once, even if multiple threads request simultaneously
        var lazySession = _sessions.GetOrAdd(speakerId, 
            new Lazy<InferenceSession>(() => CreateSession(modelPath)));

        try
        {
            return lazySession.Value;
        }
        catch (Exception ex)
        {
            // If lazy loading fails, remove from cache to allow retry
            _sessions.TryRemove(speakerId, out _);
            throw;
        }
    }

    private InferenceSession CreateSession(string modelPath)
    {
        _logger.LogInformation("Loading ONNX model for: {Path}", modelPath);
        
        if (!File.Exists(modelPath))
            throw new FileNotFoundException($"Model not found: {modelPath}");

        var options = new SessionOptions();
        options.AppendExecutionProvider_CPU();
        return new InferenceSession(modelPath, options);
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing all ONNX sessions.");
        foreach (var kvp in _sessions.Values)
        {
            if (kvp.IsValueCreated)
            {
                kvp.Value.Dispose();
            }
        }
        _sessions.Clear();
    }
}

// --- 3. TTS Service with DI ---

public interface ITtsService
{
    Task<byte[]> GenerateSpeechAsync(string text, int speakerId);
}

public class TtsService : ITtsService, IDisposable
{
    private readonly ILogger<TtsService> _logger;
    private readonly ModelCache _modelCache;
    private readonly VoiceConfig _voiceConfig;
    private readonly TtsTokenizer _tokenizer; // Assuming tokenizer is stateless or managed here

    public TtsService(
        ILogger<TtsService> logger, 
        ModelCache modelCache, 
        IOptions<VoiceConfig> voiceConfig)
    {
        _logger = logger;
        _modelCache = modelCache;
        _voiceConfig = voiceConfig.Value;
        
        // Initialize tokenizer (simplified for example)
        // In reality, you'd load a specific vocab per model
        _tokenizer = new TtsTokenizer("vocab.json"); 
    }

    public async Task<byte[]> GenerateSpeechAsync(string text, int speakerId)
    {
        // Find profile
        var profile = _voiceConfig.Profiles.FirstOrDefault(p => p.Id == speakerId);
        if (profile == null) throw new KeyNotFoundException($"Speaker ID {speakerId} not found.");

        // Requirement: Async and non-blocking
        return await Task.Run(() =>
        {
            // 1. Get Session (Cached or Loaded)
            var session = _modelCache.GetSession(speakerId, profile.ModelPath);

            // 2. Tokenize
            var tokens = _tokenizer.Tokenize(text);

            // 3. Inference (Simplified logic)
            // Create input tensor...
            // Run session...
            // Convert to WAV...
            
            // Mocking the inference result for the code snippet
            _logger.LogInformation($"Generating speech for {profile.Name}...");
            var mockAudio = new byte[1024]; 
            return mockAudio;
        });
    }

    public void Dispose()
    {
        // If TtsService owned the cache, it would dispose it. 
        // But since Cache is a Singleton in DI, the Host manages it.
    }
}

// --- 4. Host Configuration (Program.cs) ---

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Bind Configuration
                services.Configure<VoiceConfig>(context.Configuration.GetSection("VoiceSettings"));

                // Register Singleton Cache
                services.AddSingleton<ModelCache>();

                // Register TTS Service
                services.AddTransient<ITtsService, TtsService>();
            })
            .Build();

        // Demonstration of usage
        var tts = host.Services.GetRequiredService<ITtsService>();
        
        try
        {
            // This would run asynchronously
            // var audio = await tts.GenerateSpeechAsync("Hello", 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        // Cleanup on shutdown is handled by the Host container calling Dispose on Singletons
    }
}


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
using System.Text;
using System.Text.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

// --- 1. TtsTokenizer.cs ---

public class TtsTokenizer
{
    private readonly Dictionary<char, int> _vocab;
    private readonly int _padTokenId;

    public TtsTokenizer(string vocabJsonPath)
    {
        if (!File.Exists(vocabJsonPath))
            throw new FileNotFoundException($"Vocabulary file not found at {vocabJsonPath}", vocabJsonPath);

        var json = File.ReadAllText(vocabJsonPath);
        // Assuming vocab.json is a simple JSON object: { "a": 1, "b": 2, ... }
        var dict = JsonSerializer.Deserialize<Dictionary<string, int>>(json) 
                   ?? throw new InvalidDataException("Invalid vocabulary format.");

        _vocab = new Dictionary<char, int>();
        foreach (var kvp in dict)
        {
            if (kvp.Key.Length == 1)
            {
                _vocab[kvp.Key[0]] = kvp.Value;
            }
        }

        // Default to space or a specific ID if available, else 0
        _padTokenId = _vocab.TryGetValue(' ', out var spaceId) ? spaceId : 0;
    }

    public long[] Tokenize(string text)
    {
        // Filter out unsupported characters and map to IDs
        var tokens = new List<long>();
        foreach (char c in text.ToLowerInvariant())
        {
            if (_vocab.ContainsKey(c))
            {
                tokens.Add(_vocab[c]);
            }
            else
            {
                // Requirement: Handle OOV by mapping to default (space)
                tokens.Add(_padTokenId);
            }
        }
        return tokens.ToArray();
    }
}

// --- 2. OnnxInferenceService.cs ---

public class OnnxInferenceService : IDisposable
{
    private InferenceSession? _session;
    private readonly string _modelPath;

    public OnnxInferenceService(string modelPath)
    {
        _modelPath = modelPath;
    }

    public void Initialize()
    {
        if (!File.Exists(_modelPath))
            throw new FileNotFoundException($"Model file not found at {_modelPath}", _modelPath);

        // Configure SessionOptions for CPU optimizations
        var options = new SessionOptions();
        options.AppendExecutionProvider_CPU(); // Standard CPU provider
        
        // Graph optimization level (ORT_ENABLE_ALL is usually default, but explicit is good)
        options.SetGraphOptimizationLevel(GraphOptimizationLevel.ORT_ENABLE_ALL);

        _session = new InferenceSession(_modelPath, options);
    }

    public float[] RunInference(long[] tokens)
    {
        if (_session == null) throw new InvalidOperationException("Session not initialized.");

        try
        {
            // Requirement: Input shape [1, SequenceLength]
            var inputTensor = new DenseTensor<long>(tokens, [1, tokens.Length]);
            
            // The input name is usually "input" or "tokens", we can get it dynamically
            var inputName = _session.InputMetadata.Keys.First();
            
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
            };

            // Run inference
            using var results = _session.Run(inputs);
            
            // Requirement: Output shape [1, AudioLength] or [1, 1, AudioLength]
            // We assume the first output is the audio tensor
            var output = results.First().AsTensor<float>();
            
            // Flatten the tensor to a 1D array
            return output.ToArray();
        }
        catch (OnnxRuntimeException ex)
        {
            // Requirement: Catch and log
            Console.WriteLine($"[ERROR] ONNX Runtime Exception: {ex.Message}");
            throw; // Re-throw to be handled by orchestrator
        }
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}

// --- 3. AudioWriter.cs ---

public static class AudioWriter
{
    public static byte[] GenerateWavFile(float[] audioData, int sampleRate = 22050)
    {
        // Requirement: Convert floats (-1.0 to 1.0) to 16-bit PCM (short)
        // Clamp values to prevent distortion
        var pcmData = new short[audioData.Length];
        for (int i = 0; i < audioData.Length; i++)
        {
            var sample = audioData[i];
            if (sample > 1.0f) sample = 1.0f;
            if (sample < -1.0f) sample = -1.0f;
            pcmData[i] = (short)(sample * 32767);
        }

        // Requirement: Manually construct RIFF WAV header (44 bytes)
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        // RIFF Header
        writer.Write(Encoding.ASCII.GetBytes("RIFF")); // ChunkID
        writer.Write(36 + pcmData.Length * 2); // ChunkSize (36 + SubChunk2Size)
        writer.Write(Encoding.ASCII.GetBytes("WAVE")); // Format

        // fmt Subchunk
        writer.Write(Encoding.ASCII.GetBytes("fmt ")); // Subchunk1ID
        writer.Write(16); // Subchunk1Size (16 for PCM)
        writer.Write((short)1); // AudioFormat (1 = PCM)
        writer.Write((short)1); // NumChannels (1 = Mono)
        writer.Write(sampleRate); // SampleRate
        writer.Write(sampleRate * 2); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
        writer.Write((short)2); // BlockAlign (NumChannels * BitsPerSample/8)
        writer.Write((short)16); // BitsPerSample

        // data Subchunk
        writer.Write(Encoding.ASCII.GetBytes("data")); // Subchunk2ID
        writer.Write(pcmData.Length * 2); // Subchunk2Size

        // Write PCM data
        foreach (var sample in pcmData)
        {
            writer.Write(sample);
        }

        return stream.ToArray();
    }
}

// --- 4. Program.cs (Orchestration) ---

public class Program
{
    public static void Main(string[] args)
    {
        // Paths (Mock paths for demonstration)
        string modelPath = "model.onnx";
        string vocabPath = "vocab.json";
        string outputPath = "output.wav";
        string inputText = "Hello world, this is a test of the TTS system.";

        // Create dummy vocab for testing if file doesn't exist
        if (!File.Exists(vocabPath))
        {
            var dummyVocab = new Dictionary<string, int> 
            { 
                { " ", 0 }, { "h", 1 }, { "e", 2 }, { "l", 3 }, { "o", 4 }, 
                { "w", 5 }, { "r", 6 }, { "d", 7 }, { "t", 8 }, { "s", 9 }, 
                { "a", 10 }, { "i", 11 }, { "c", 12 }, { "m", 13 }, { "p", 14 }, 
                { ".", 15 }, { ",", 16 } 
            };
            File.WriteAllText(vocabPath, System.Text.Json.JsonSerializer.Serialize(dummyVocab));
        }

        try
        {
            // 1. Tokenize
            var tokenizer = new TtsTokenizer(vocabPath);
            var tokens = tokenizer.Tokenize(inputText);
            Console.WriteLine($"Tokenized '{inputText}' to {tokens.Length} tokens.");

            // 2. Inference
            // Note: We wrap inference in a disposable block to ensure resources are freed
            float[] audioData;
            using (var inferenceService = new OnnxInferenceService(modelPath))
            {
                // In a real scenario, the model file must exist. 
                // We wrap this to catch the specific FileNotFoundException required.
                try 
                {
                    inferenceService.Initialize();
                    audioData = inferenceService.RunInference(tokens);
                }
                catch (FileNotFoundException ex) when (ex.Message.Contains("Model file"))
                {
                    // Requirement: Throw custom exception if model missing
                    // (Actually, the Initialize method throws this, we catch it here to prove logic)
                    Console.WriteLine($"Critical Error: {ex.Message}");
                    // Since we can't actually run without a real model file in this text environment,
                    // we will mock the audio data to allow the rest of the pipeline to be demonstrated.
                    Console.WriteLine("!!! MOCKING AUDIO DATA FOR DEMONSTRATION (Model file missing) !!!");
                    audioData = new float[22050]; // 1 second of silence
                    for (int i = 0; i < audioData.Length; i++) 
                        audioData[i] = (float)Math.Sin(i * 0.1); // Sine wave
                }
            }

            // 3. Post-Process & Write
            var wavBytes = AudioWriter.GenerateWavFile(audioData);
            File.WriteAllBytes(outputPath, wavBytes);
            Console.WriteLine($"Audio saved to {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Pipeline failed: {ex.Message}");
        }
    }
}

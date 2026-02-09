
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalTtsDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Define the input text
            string textInput = "Hello, world! This is local TTS.";
            Console.WriteLine($"Input Text: \"{textInput}\"");

            // 2. Tokenize: Convert text to phonemes/IDs (Simulated)
            // In a real app, this uses a tokenizer model or a dictionary.
            var tokenIds = TokenizeText(textInput);
            Console.WriteLine($"Generated Tokens: {string.Join(", ", tokenIds)}");

            // 3. Acoustic Model Inference: Tokens -> Spectrogram (Mel-Spectrogram)
            // This is the "Brain" of the TTS model (e.g., VITS or Piper encoder).
            var melSpectrogram = RunAcousticModel(tokenIds);
            Console.WriteLine($"Generated Spectrogram Shape: {melSpectrogram.Shape[0]}x{melSpectrogram.Shape[1]}");

            // 4. Vocoder Inference: Spectrogram -> Raw Audio Waveform
            // This converts spectral features into time-domain audio (e.g., WaveRNN or HiFi-GAN).
            var audioWaveform = RunVocoder(melSpectrogram);
            Console.WriteLine($"Generated Audio Samples: {audioWaveform.Length}");

            // 5. Save/Play Audio (Simulated)
            // In a real app, you would write these floats to a .wav file.
            SaveAudioToFile(audioWaveform, "output.wav");
            Console.WriteLine("Audio saved to 'output.wav' (Simulated).");
        }

        // --- Step 1: Text Tokenization ---
        static List<int> TokenizeText(string text)
        {
            // SIMULATION: A real TTS system uses a tokenizer (like BPE or phonemizer).
            // Here, we map characters to dummy IDs for demonstration.
            // IDs 1-26 might represent 'a'-'z', 27 is space, etc.
            var tokens = new List<int>();
            foreach (char c in text.ToLower())
            {
                if (c >= 'a' && c <= 'z') tokens.Add(c - 'a' + 1);
                else if (c == ' ') tokens.Add(27);
                else tokens.Add(28); // punctuation
            }
            return tokens;
        }

        // --- Step 2: Acoustic Model (Text -> Mel-Spectrogram) ---
        static DenseTensor<float> RunAcousticModel(List<int> tokenIds)
        {
            // SIMULATION: In reality, we load an ONNX model file.
            // var session = new InferenceSession("tts_model.onnx");
            
            // Create dummy input tensor based on token count
            // Shape: [BatchSize=1, SequenceLength=N]
            var inputTensor = new DenseTensor<float>(new[] { 1, tokenIds.Count });
            for (int i = 0; i < tokenIds.Count; i++)
                inputTensor[0, i] = tokenIds[i];

            // SIMULATION: Run Inference
            // var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", inputTensor) };
            // var results = session.Run(inputs);
            // var outputTensor = results.First().AsTensor<float>();
            
            // MOCK RESULT: Generate a dummy Mel-Spectrogram
            // Shape: [BatchSize=1, MelChannels=80, TimeSteps=TokenCount * 2]
            int melChannels = 80;
            int timeSteps = tokenIds.Count * 2; 
            var mockMelSpectrogram = new DenseTensor<float>(new[] { 1, melChannels, timeSteps });
            
            // Fill with dummy data (simulating learned features)
            for (int t = 0; t < timeSteps; t++)
            {
                for (int m = 0; m < melChannels; m++)
                {
                    // Create a simple sine wave pattern to simulate audio features
                    mockMelSpectrogram[0, m, t] = (float)Math.Sin(t * 0.2 + m * 0.1);
                }
            }

            return mockMelSpectrogram;
        }

        // --- Step 3: Vocoder (Mel-Spectrogram -> Audio) ---
        static float[] RunVocoder(DenseTensor<float> melSpectrogram)
        {
            // SIMULATION: In reality, load the vocoder ONNX model (e.g., HiFi-GAN).
            // var session = new InferenceSession("vocoder.onnx");

            // MOCK RESULT: Generate dummy audio samples
            // Input Shape: [1, 80, TimeSteps]
            // Output Shape: [1, AudioLength]
            
            int timeSteps = (int)melSpectrogram.Dimensions[2];
            int audioLength = timeSteps * 256; // Upsampling factor (e.g., 256x for HiFi-GAN)
            
            var audioBuffer = new float[audioLength];

            // Generate a dummy waveform based on the mel input
            for (int i = 0; i < audioLength; i++)
            {
                // Modulate amplitude by the mel features
                float feature = melSpectrogram[0, i % 80, i / 256];
                audioBuffer[i] = (float)Math.Sin(i * 0.05) * feature * 0.5f;
            }

            return audioBuffer;
        }

        // --- Step 4: Save Audio (Mock) ---
        static void SaveAudioToFile(float[] audioData, string filename)
        {
            // In a real application, you would use a library like NAudio or 
            // manually construct a WAV header to write the raw PCM data.
            // This is just a placeholder to show where the data goes.
            Console.WriteLine($"[Mock] Writing {audioData.Length} samples to {filename}...");
        }
    }
}

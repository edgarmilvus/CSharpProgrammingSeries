
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalTTSApplication
{
    class Program
    {
        // Entry point of the application.
        static void Main(string[] args)
        {
            Console.WriteLine("=== Local Text-to-Speech Application ===");
            Console.WriteLine("Initializing TTS Pipeline...");

            // 1. Initialize the TTS Service
            // In a real scenario, we would load specific ONNX model paths here.
            // For this example, we simulate the existence of models.
            var ttsService = new TTSService();

            // 2. Define the input text
            string inputText = "Hello! This is a demonstration of local text-to-speech inference using ONNX Runtime.";

            Console.WriteLine($"Input Text: \"{inputText}\"");

            try
            {
                // 3. Execute the TTS Pipeline
                // This involves text normalization, acoustic model inference, and vocoder inference.
                byte[] audioData = ttsService.GenerateSpeech(inputText);

                // 4. Save the output
                string outputPath = "output_speech.wav";
                File.WriteAllBytes(outputPath, audioData);

                Console.WriteLine($"\nSuccess! Audio saved to: {Path.GetFullPath(outputPath)}");
                Console.WriteLine("You can now play this file using any standard media player.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError during TTS generation: {ex.Message}");
                Console.WriteLine("Ensure ONNX models are available at the specified paths.");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Manages the Text-to-Speech pipeline.
    /// Handles text processing, ONNX model inference, and audio header construction.
    /// </summary>
    public class TTSService
    {
        // Placeholder paths for ONNX models.
        // In a real implementation, these would be paths to 'piper-phonemize.onnx' (Acoustic) and 'hifigan.onnx' (Vocoder).
        private const string AcousticModelPath = "acoustic_model.onnx";
        private const string VocoderModelPath = "vocoder_model.onnx";

        // Mock phoneme mapping for demonstration purposes.
        // Real systems use complex tokenizers (e.g., espeak-ng).
        private Dictionary<char, int> _phonemeMap;

        public TTSService()
        {
            InitializePhonemeMap();
        }

        /// <summary>
        /// Initializes a mock phoneme-to-id mapping.
        /// </summary>
        private void InitializePhonemeMap()
        {
            _phonemeMap = new Dictionary<char, int>
            {
                {'a', 1}, {'b', 2}, {'c', 3}, {'d', 4}, {'e', 5}, {'f', 6}, {'g', 7}, {'h', 8},
                {'i', 9}, {'j', 10}, {'k', 11}, {'l', 12}, {'m', 13}, {'n', 14}, {'o', 15}, {'p', 16},
                {'q', 17}, {'r', 18}, {'s', 19}, {'t', 20}, {'u', 21}, {'v', 22}, {'w', 23}, {'x', 24},
                {'y', 25}, {'z', 26}, {' ', 0}, {'.', 0}, {',', 0}, {'!', 0}, {'?', 0}
            };
        }

        /// <summary>
        /// Main pipeline method to convert text to audio bytes.
        /// </summary>
        /// <param name="text">The input text string.</param>
        /// <returns>Raw audio data formatted as a WAV file.</returns>
        public byte[] GenerateSpeech(string text)
        {
            // Step 1: Text Normalization & Tokenization
            // Convert raw text into a sequence of integers (phonemes or tokens).
            int[] tokens = TextToTokens(text);

            // Step 2: Acoustic Model Inference
            // Convert tokens into Mel-Spectrograms (audio features).
            // In a real app, this runs the 'AcousticModelPath' ONNX model.
            float[,] melSpectrogram = RunAcousticModel(tokens);

            // Step 3: Vocoder Inference
            // Convert Mel-Spectrograms into raw audio waveforms.
            // In a real app, this runs the 'VocoderModelPath' ONNX model.
            float[] audioWaveform = RunVocoder(melSpectrogram);

            // Step 4: Audio Post-Processing & Header Generation
            // Convert float waveform to 16-bit PCM and wrap in WAV header.
            return CreateWavFile(audioWaveform);
        }

        /// <summary>
        /// Simulates text normalization and tokenization.
        /// </summary>
        private int[] TextToTokens(string text)
        {
            List<int> tokens = new List<int>();
            string normalized = text.ToLower();

            foreach (char c in normalized)
            {
                if (_phonemeMap.ContainsKey(c))
                {
                    tokens.Add(_phonemeMap[c]);
                }
            }

            // Add padding token to ensure sequence length for the model
            while (tokens.Count < 100) tokens.Add(0);
            
            // Truncate if too long for this mock model
            if (tokens.Count > 200) tokens.RemoveRange(200, tokens.Count - 200);

            return tokens.ToArray();
        }

        /// <summary>
        /// Simulates the Acoustic Model (e.g., Piper/VITS) inference.
        /// Returns a Mel-Spectrogram (Frequency x Time).
        /// </summary>
        private float[,] RunAcousticModel(int[] tokens)
        {
            // In a real scenario, we would use:
            // using var session = new InferenceSession(AcousticModelPath);
            // var inputs = new List<NamedOnnxValue> { ... };
            // var results = session.Run(inputs);
            
            // SIMULATION: Generate a dummy Mel-Spectrogram (64 frequency bins x 100 time frames)
            int freqBins = 64;
            int timeFrames = tokens.Length;
            float[,] mel = new float[freqBins, timeFrames];

            // Fill with dummy data representing frequency intensities
            Random rand = new Random(42); // Fixed seed for reproducibility
            for (int t = 0; t < timeFrames; t++)
            {
                for (int f = 0; f < freqBins; f++)
                {
                    // Create a pattern based on token position
                    float val = (float)(Math.Sin(t * 0.1) * Math.Cos(f * 0.2) + rand.NextDouble() * 0.1);
                    mel[f, t] = val;
                }
            }

            return mel;
        }

        /// <summary>
        /// Simulates the Vocoder (e.g., HiFi-GAN) inference.
        /// Converts Mel-Spectrogram to Audio Waveform.
        /// </summary>
        private float[] RunVocoder(float[,] melSpectrogram)
        {
            // In a real scenario, we would use:
            // using var session = new InferenceSession(VocoderModelPath);
            
            // SIMULATION: Generate a dummy waveform.
            // Usually, the output length is mel_frames * hop_length.
            int melFrames = melSpectrogram.GetLength(1);
            int waveformLength = melFrames * 256; // Standard hop length
            float[] waveform = new float[waveformLength];

            Random rand = new Random(42); // Consistent noise
            for (int i = 0; i < waveformLength; i++)
            {
                // Simple synthesis: Sine wave + Noise
                // This mimics the reconstruction of audio from spectral features.
                float t = i / 44100.0f; // Assuming 44.1kHz sample rate
                waveform[i] = (float)(Math.Sin(2 * Math.PI * 440 * t) * 0.1 + (rand.NextDouble() - 0.5) * 0.05);
                
                // Apply a simple envelope to avoid clipping
                if (i < 1000) waveform[i] *= (i / 1000.0f);
                if (i > waveformLength - 1000) waveform[i] *= ((waveformLength - i) / 1000.0f);
            }

            return waveform;
        }

        /// <summary>
        /// Converts float audio data to 16-bit PCM and adds WAV header.
        /// </summary>
        private byte[] CreateWavFile(float[] audioData)
        {
            int sampleRate = 44100;
            int bitsPerSample = 16;
            int blockAlign = (bitsPerSample / 8) * 1; // Mono
            int byteRate = sampleRate * blockAlign;
            
            // Convert float [-1.0, 1.0] to short [-32768, 32767]
            short[] pcmData = new short[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                float sample = audioData[i];
                if (sample > 1.0f) sample = 1.0f;
                if (sample < -1.0f) sample = -1.0f;
                pcmData[i] = (short)(sample * 32767);
            }

            // Calculate total size
            int dataSize = pcmData.Length * 2;
            int fileSize = 36 + dataSize; // 36 bytes for header chunk excluding RIFF ID

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // RIFF Header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(fileSize);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt Subchunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1Size (16 for PCM)
                writer.Write((short)1); // AudioFormat (1 for PCM)
                writer.Write((short)1); // NumChannels (Mono)
                writer.Write(sampleRate); // SampleRate
                writer.Write(byteRate); // ByteRate
                writer.Write((short)blockAlign); // BlockAlign
                writer.Write((short)bitsPerSample); // BitsPerSample

                // data Subchunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);

                // Write PCM Data
                foreach (short val in pcmData)
                {
                    writer.Write(val);
                }

                return stream.ToArray();
            }
        }
    }
}

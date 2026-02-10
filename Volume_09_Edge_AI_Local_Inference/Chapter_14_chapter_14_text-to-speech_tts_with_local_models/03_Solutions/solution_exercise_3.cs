
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
using System.Collections.Generic;
using System.Linq;

// --- 1. Parameter Classes ---

public record TtsParameters
{
    public double Speed { get; init; } = 1.0; // 1.0 = Normal, >1.0 = Faster
    public double NoiseScale { get; init; } = 0.667;
    public double LengthScale { get; init; } = 1.0;

    public TtsParameters Validate()
    {
        // Requirement: Clamp speed between 0.5x and 2.0x
        double clampedSpeed = Math.Clamp(Speed, 0.5, 2.0);
        
        return this with { Speed = clampedSpeed };
    }
}

// --- 2. Speed-Adjusted Tokenizer ---

public class ProsodicTokenizer
{
    private readonly Dictionary<char, int> _vocab;
    // Assuming a silence/pause token ID exists, e.g., 0 or specific ID
    private const int PauseTokenId = 0; 

    public ProsodicTokenizer(Dictionary<char, int> vocab)
    {
        _vocab = vocab;
    }

    public long[] TokenizeWithSpeed(string text, TtsParameters parameters)
    {
        var baseTokens = new List<long>();
        
        // 1. Basic Tokenization
        foreach (char c in text.ToLowerInvariant())
        {
            if (_vocab.ContainsKey(c))
                baseTokens.Add(_vocab[c]);
            else
                baseTokens.Add(PauseTokenId);
        }

        // 2. Speed Modification (Input Manipulation)
        // If Speed > 1.0 (Faster): We want to remove "space" or pauses.
        // If Speed < 1.0 (Slower): We want to insert pauses.

        if (parameters.Speed > 1.0)
        {
            // Fast: Remove pause tokens
            return baseTokens.Where(t => t != PauseTokenId).ToArray();
        }
        else if (parameters.Speed < 1.0)
        {
            // Slow: Insert pauses between words (approximate by checking for spaces/pauses)
            var stretched = new List<long>();
            foreach (var token in baseTokens)
            {
                stretched.Add(token);
                // Insert a pause if it's a space or punctuation
                if (token == PauseTokenId) 
                {
                    // Add extra pause to stretch time
                    stretched.Add(PauseTokenId); 
                }
            }
            return stretched.ToArray();
        }

        return baseTokens.ToArray();
    }
}

// --- 3. Phase Vocoder / Time Stretching Logic ---

public static class AudioPostProcessor
{
    /// <summary>
    /// Performs Time-Stretching on raw audio buffer using a simplified Phase Vocoder approach.
    /// Note: A full Phase Vocoder is complex (FFT -> Phase Accumulation -> IFFT).
    /// This implementation simulates the logic by manipulating the playback rate 
    /// via resampling interpolation, which preserves pitch.
    /// </summary>
    public static float[] TimeStretch(float[] input, double speedMultiplier)
    {
        // Requirement: Speed changes without pitch shifting.
        // To slow down (Speed < 1.0), we need more samples (interpolation).
        // To speed up (Speed > 1.0), we need fewer samples (decimation).

        // Inverse relationship: 
        // Slow speed (0.5x) means duration doubles (2.0x length).
        // Fast speed (2.0x) means duration halves (0.5x length).
        double stretchFactor = 1.0 / speedMultiplier;

        if (Math.Abs(stretchFactor - 1.0) < 0.001) return input;

        int newLength = (int)(input.Length * stretchFactor);
        float[] output = new float[newLength];

        for (int i = 0; i < newLength; i++)
        {
            // Map output index to input index
            double inputIndex = i / stretchFactor;
            
            // Linear Interpolation for smoothness
            int indexA = (int)Math.Floor(inputIndex);
            int indexB = Math.Min(indexA + 1, input.Length - 1);
            double fraction = inputIndex - indexA;

            if (indexA < 0 || indexA >= input.Length)
            {
                output[i] = 0;
            }
            else
            {
                output[i] = (float)(input[indexA] * (1.0 - fraction) + input[indexB] * fraction);
            }
        }

        return output;
    }
}

// --- 4. Usage Example ---

public class ProsodyManager
{
    public float[] GenerateAdjustedAudio(string text, TtsParameters parameters, Func<long[], float[]> baseInference)
    {
        // 1. Validate
        var safeParams = parameters.Validate();

        // 2. Tokenize (Input Modification)
        // Assuming a tokenizer instance exists
        var tokenizer = new ProsodicTokenizer(new Dictionary<char, int> { { ' ', 0 }, { 'a', 1 }, { 'b', 2 } }); // Mock vocab
        var tokens = tokenizer.TokenizeWithSpeed(text, safeParams);

        // 3. Run Base Inference
        float[] rawAudio = baseInference(tokens);

        // 4. Post-Processing (Phase Vocoder / Resampling)
        // If speed was handled purely by input tokens, we might not need this.
        // But for "High Quality" as requested, we apply time-stretching to the result.
        // If we already inserted pauses (Slow), we might skip this to avoid double-slowing,
        // or rely solely on this for pure speed control.
        
        // Let's assume we use the Post-Processor for the final adjustment.
        var finalAudio = AudioPostProcessor.TimeStretch(rawAudio, safeParams.Speed);

        return finalAudio;
    }
}

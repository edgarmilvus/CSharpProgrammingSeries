
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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

public class LargeLanguageModel
{
    // The event declaration using the standard .NET EventHandler generic delegate
    // This event is the "Bell" in our restaurant analogy.
    public event EventHandler<InferenceCompletedEventArgs>? ModelFinishedThinking;

    // This method simulates the heavy AI calculation running on a background thread
    public async Task StartInferenceAsync(string prompt)
    {
        // ... (Simulate background processing) ...
        await Task.Delay(2000); 

        // Prepare the data to pass to observers
        var resultData = new { Response = "Generated text based on: " + prompt };
        var metadata = new InferenceMetadata 
        { 
            TokensGenerated = 150, 
            Duration = TimeSpan.FromSeconds(2.0), 
            ModelVersion = "v2.1-beta" 
        };
        
        var args = new InferenceCompletedEventArgs(resultData, metadata, true);

        // RAISING THE EVENT (The Notification)
        // We use the ?.Invoke operator (Null-conditional operator) to safely raise the event.
        // If no subscribers exist (null), nothing happens. No crash.
        ModelFinishedThinking?.Invoke(this, args);
    }
}

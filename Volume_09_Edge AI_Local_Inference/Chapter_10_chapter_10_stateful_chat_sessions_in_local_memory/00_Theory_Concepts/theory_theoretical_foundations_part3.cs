
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

# Source File: theory_theoretical_foundations_part3.cs
# Description: Theoretical Foundations
# ==========================================

// Conceptual interface representing the inference engine
public interface ILocalInferenceEngine
{
    Task<string> InferAsync(ReadOnlyMemory<int> tokens, CancellationToken ct);
}

// The state manager orchestrates the flow
public class StatefulChatSession
{
    private readonly IConversationState _state;
    private readonly ILocalInferenceEngine _engine;
    private readonly ITokenizer _tokenizer;

    public async Task<string> ChatAsync(string userInput)
    {
        // 1. Update State
        _state.AddMessage(ChatRole.User, userInput);

        // 2. Prepare Input (Tokenization)
        string prompt = _state.BuildPrompt();
        var tokens = _tokenizer.Encode(prompt);

        // 3. Enforce Constraints (Truncation if needed)
        if (tokens.Length > _maxContextLength)
        {
            _state.Truncate(_maxContextLength - tokens.Length); // Logic to remove old messages
            // Re-encode after truncation
            prompt = _state.BuildPrompt();
            tokens = _tokenizer.Encode(prompt);
        }

        // 4. Inference
        string response = await _engine.InferAsync(tokens, CancellationToken.None);

        // 5. Update State with Response
        _state.AddMessage(ChatRole.Assistant, response);

        return response;
    }
}


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

// ---------------------------------------------------------
// 1. THE CONTRACT (Interface)
// ---------------------------------------------------------
// We define the 'ILanguageModel' interface. This is the blueprint
// that all AI models must follow. It contains no implementation details,
// only the promise that specific methods exist.
public interface ILanguageModel
{
    // A method to convert raw text into tokens (numbers the AI understands).
    string Tokenize(string input);

    // A method to generate a response based on input tokens.
    string Generate(string tokens);
}

// ---------------------------------------------------------
// 2. IMPLEMENTATION A: LEGACY RNN MODEL
// ---------------------------------------------------------
// This class implements the interface. It represents an older,
// perhaps less efficient model. Note that it adheres strictly to
// the contract defined by ILanguageModel.
public class LegacyRNNModel : ILanguageModel
{
    public string Tokenize(string input)
    {
        // Simulate tokenization for an RNN (e.g., character-based encoding).
        return "RNN_TOKENS_" + input.Length;
    }

    public string Generate(string tokens)
    {
        // Simulate a response from the RNN.
        return "RNN Response based on " + tokens;
    }
}

// ---------------------------------------------------------
// 3. IMPLEMENTATION B: ADVANCED TRANSFORMER MODEL
// ---------------------------------------------------------
// This is a newer, more complex model. It also implements the interface.
// It might use heavy matrix math internally (hidden from the user),
// but externally, it looks exactly like the RNN to the rest of the system.
public class TransformerModel : ILanguageModel
{
    public string Tokenize(string input)
    {
        // Simulate complex sub-word tokenization.
        return "TRANSFORMER_BPE_" + input;
    }

    public string Generate(string tokens)
    {
        // Simulate a high-quality, context-aware response.
        return "Transformer Response: Hello! I processed " + tokens;
    }
}

// ---------------------------------------------------------
// 4. THE CONSUMER (ChatBot System)
// ---------------------------------------------------------
// The ChatBot class does NOT know if it is using an RNN or a Transformer.
// It only knows it has an 'ILanguageModel'. This is the key to modularity.
public class ChatBot
{
    // The dependency is injected via the constructor.
    private readonly ILanguageModel _aiModel;

    public ChatBot(ILanguageModel model)
    {
        _aiModel = model;
    }

    public string HandleMessage(string userMessage)
    {
        // Step 1: Tokenize the input using the injected model.
        string tokens = _aiModel.Tokenize(userMessage);

        // Step 2: Generate a response using the same injected model.
        string response = _aiModel.Generate(tokens);

        return response;
    }
}

// ---------------------------------------------------------
// 5. MAIN PROGRAM (Runtime Swapping)
// ---------------------------------------------------------
// This demonstrates the power of the interface.
public class Program
{
    static void Main()
    {
        // Scenario A: We start with the Legacy RNN model.
        ILanguageModel legacyModel = new LegacyRNNModel();
        ChatBot botV1 = new ChatBot(legacyModel);

        Console.WriteLine("--- Starting Bot V1 (Legacy RNN) ---");
        Console.WriteLine(botV1.HandleMessage("Where is my order?"));
        Console.WriteLine();

        // Scenario B: We upgrade the system to a Transformer model.
        // Notice: We did NOT change the ChatBot class at all.
        // We simply swapped the implementation of the interface.
        ILanguageModel newModel = new TransformerModel();
        ChatBot botV2 = new ChatBot(newModel);

        Console.WriteLine("--- Upgrading Bot to V2 (Transformer) ---");
        Console.WriteLine(botV2.HandleMessage("Where is my order?"));
    }
}

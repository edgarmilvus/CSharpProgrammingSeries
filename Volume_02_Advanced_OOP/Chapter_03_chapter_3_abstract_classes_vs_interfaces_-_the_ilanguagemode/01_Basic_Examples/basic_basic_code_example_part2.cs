
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

// Source File: basic_basic_code_example_part2.cs
// Description: Basic Code Example
// ==========================================

// Implementation 1: A simple rule-based model (e.g., for testing or legacy systems).
public class RuleBasedModel : ILanguageModel
{
    // Converts input to uppercase to simulate "processing" and splits by space.
    public string[] Tokenize(string input)
    {
        // Logic: Uppercase and split.
        return input.ToUpper().Split(' ');
    }

    // Generates a response based on specific keywords.
    public string Generate(string[] tokens)
    {
        // Check for specific keywords to return a canned response.
        foreach (string token in tokens)
        {
            if (token == "HELLO")
            {
                return "Greetings, human!";
            }
        }
        return "I don't understand.";
    }
}

// Implementation 2: A simulated Transformer model (complex logic placeholder).
public class TransformerModel : ILanguageModel
{
    // Simulates a tokenizer that adds special BOS/EOS tokens.
    public string[] Tokenize(string input)
    {
        // Logic: Wrap input in special tokens to simulate a tokenizer like BPE.
        return new string[] { "<BOS>", input, "<EOS>" };
    }

    // Simulates a generative process (e.g., next token prediction).
    public string Generate(string[] tokens)
    {
        // Logic: If we have enough tokens, generate a "complex" response.
        if (tokens.Length >= 3)
        {
            return "Processed by Transformer: " + tokens[1];
        }
        return "Insufficient context for generation.";
    }
}

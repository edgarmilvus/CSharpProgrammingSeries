
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

public class ModelPipeline
{
    private readonly ILanguageModel _model;

    // Constructor Injection: Depends on the abstraction (ILanguageModel), 
    // not the concrete class. This is Dependency Inversion.
    public ModelPipeline(ILanguageModel model)
    {
        _model = model;
    }

    public string ExecutePipeline(string input)
    {
        // Step 1: Tokenize the input text
        List<string> tokens = _model.Tokenize(input);

        // Step 2: Train the model (using the input tokens as a mini-corpus)
        _model.Train(tokens);

        // Step 3: Generate a response using the tokenized input
        // We pass the tokens and a limit of 10 tokens for generation
        string result = _model.Generate(tokens, 10);

        return result;
    }
}

public class Program
{
    public static void Main()
    {
        // Scenario A: Using Transformer
        // We declare the variable as ILanguageModel, but instantiate TransformerModel
        ILanguageModel transformer = new TransformerModel();
        ModelPipeline pipelineA = new ModelPipeline(transformer);
        string outputA = pipelineA.ExecutePipeline("Hello AI World");
        Console.WriteLine($"Output A: {outputA}\n");

        // Scenario B: Using RNN
        // We swap the concrete implementation without changing the Pipeline logic
        ILanguageModel rnn = new RNNModel();
        ModelPipeline pipelineB = new ModelPipeline(rnn);
        string outputB = pipelineB.ExecutePipeline("Hello AI World");
        Console.WriteLine($"Output B: {outputB}");
    }
}

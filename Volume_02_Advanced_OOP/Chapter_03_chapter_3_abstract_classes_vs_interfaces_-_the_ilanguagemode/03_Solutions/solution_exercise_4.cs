
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
using System.Collections.Generic;

public class EnsembleModel : ILanguageModel
{
    // Composition: Holding references to other implementations of the interface
    private readonly ILanguageModel _modelA;
    private readonly ILanguageModel _modelB;

    public string ModelName => $"Ensemble({_modelA.ModelName} + {_modelB.ModelName})";

    public EnsembleModel(ILanguageModel modelA, ILanguageModel modelB)
    {
        _modelA = modelA;
        _modelB = modelB;
    }

    public List<string> Tokenize(string input)
    {
        // Strategy: Use Model A's tokenizer as the standard for the ensemble
        return _modelA.Tokenize(input);
    }

    public void Train(List<string> corpus)
    {
        // Requirement: Both models must be trained
        Console.WriteLine("--- Starting Ensemble Training ---");
        _modelA.Train(corpus);
        _modelB.Train(corpus);
        Console.WriteLine("--- Ensemble Training Complete ---");
    }

    public string Generate(List<string> tokens, int maxTokens)
    {
        // Requirement: Generate from both and combine
        string genA = _modelA.Generate(tokens, maxTokens);
        string genB = _modelB.Generate(tokens, maxTokens);

        // Combining the results for a "voting" or "blending" simulation
        return $"Ensemble[A: {genA} | B: {genB}]";
    }
}

public class Program
{
    public static void Main()
    {
        // 1. Create individual models
        ILanguageModel transformer = new TransformerModel();
        ILanguageModel rnn = new RNNModel();

        // 2. Create the Ensemble (Composition)
        // The Ensemble itself implements ILanguageModel, so it can be treated as a single model
        ILanguageModel ensemble = new EnsembleModel(transformer, rnn);

        // 3. Inject into the existing pipeline
        // The pipeline doesn't know this is an ensemble; it just sees an ILanguageModel
        ModelPipeline pipeline = new ModelPipeline(ensemble);

        // 4. Execute
        string result = pipeline.ExecutePipeline("Complex AI Systems");
        
        Console.WriteLine($"\nFinal Result: {result}");
    }
}

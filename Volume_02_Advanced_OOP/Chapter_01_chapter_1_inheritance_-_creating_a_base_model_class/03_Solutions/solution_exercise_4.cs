
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

// Assuming Model, LinearRegressionModel, and PolynomialRegressionModel are defined 
// (Inherit from the definitions in previous exercises, ensuring ModelId is abstract)

// 1. Singleton Model Registry
public sealed class ModelRegistry
{
    // Thread-safe singleton implementation (simplified for intermediate level)
    private static readonly ModelRegistry _instance = new ModelRegistry();
    
    // Private dictionary to store models
    private readonly Dictionary<string, Model> _models = new Dictionary<string, Model>();

    // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
    static ModelRegistry() { }

    // Private constructor prevents external instantiation
    private ModelRegistry() { }

    public static ModelRegistry Instance => _instance;

    // 2. Register Method
    public void Register(Model model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        
        // Ensure the model has an ID (assuming ModelId is populated)
        if (string.IsNullOrEmpty(model.ModelId))
            throw new InvalidOperationException("Model must have a valid ID to be registered.");

        if (_models.ContainsKey(model.ModelId))
        {
            Console.WriteLine($"Warning: Model with ID {model.ModelId} already registered. Overwriting.");
        }

        _models[model.ModelId] = model;
        Console.WriteLine($"Model {model.ModelId} registered.");
    }

    // 3. Get Method
    public Model GetModel(string id)
    {
        if (_models.TryGetValue(id, out var model))
        {
            return model;
        }
        throw new KeyNotFoundException($"Model with ID {id} not found in registry.");
    }

    // 4. Batch Predict
    public void BatchPredict(double[][] input, string[] modelIds)
    {
        Console.WriteLine("\n--- Starting Batch Prediction ---");
        foreach (var id in modelIds)
        {
            try
            {
                Model model = GetModel(id);
                double[] predictions = model.Predict(input);
                Console.WriteLine($"Model {id} Prediction: [{string.Join(", ", predictions)}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error predicting with model {id}: {ex.Message}");
            }
        }
        Console.WriteLine("--- Batch Prediction Complete ---");
    }
}

// 5. Refactored Main Method
public class Program
{
    public static void Main()
    {
        // Instantiate models (IDs are auto-generated in the Model class from Exercise 3)
        var linearModel = new LinearRegressionModel();
        var polyModel = new PolynomialRegressionModel();

        // Access the Singleton Registry
        var registry = ModelRegistry.Instance;

        // Register models
        registry.Register(linearModel);
        registry.Register(polyModel);

        // Prepare data
        double[][] inputData = new double[][] { new double[] { 5 }, new double[] { 10 } };
        string[] targetIds = new string[] { linearModel.ModelId, polyModel.ModelId };

        // Perform Batch Operation via Registry
        registry.BatchPredict(inputData, targetIds);

        // Demonstrate Retrieval
        Model retrievedModel = registry.GetModel(linearModel.ModelId);
        Console.WriteLine($"\nRetrieved model status: {retrievedModel.Status}");
    }
}

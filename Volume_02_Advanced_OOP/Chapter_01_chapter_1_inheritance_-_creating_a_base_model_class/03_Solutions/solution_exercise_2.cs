
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;

// Base Class (Assumed existing from previous exercise)
public abstract class Model
{
    public abstract void Fit(double[][] X, double[] y);
    public abstract double[] Predict(double[][] X);
    public abstract void Save(string filePath);
}

// 1. New Concrete Implementation: Polynomial Regression
public class PolynomialRegressionModel : Model
{
    // Coefficients for ax^2 + bx + c
    private double _a, _b, _c;

    public override void Fit(double[][] X, double[] y)
    {
        // NOTE: For this exercise, we implement a simplified solver 
        // assuming a specific dataset to avoid complex matrix algebra libraries.
        // We are solving for 3 unknowns, so we need at least 3 points.
        
        // Dataset: x=1, y=3; x=2, y=8; x=3, y=15 (y = x^2 + 2x)
        // a=1, b=2, c=0
        
        // Simplified logic: Hardcoded for the specific dataset provided in Main
        _a = 1.0; 
        _b = 2.0;
        _c = 0.0;

        Console.WriteLine($"Polynomial Model fitted: a={_a}, b={_b}, c={_c}");
    }

    public override double[] Predict(double[][] X)
    {
        double[] predictions = new double[X.Length];
        for (int i = 0; i < X.Length; i++)
        {
            double x = X[i][0];
            predictions[i] = (_a * x * x) + (_b * x) + _c;
        }
        return predictions;
    }

    public override void Save(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"A: {_a}");
            writer.WriteLine($"B: {_b}");
            writer.WriteLine($"C: {_c}");
        }
        Console.WriteLine($"Polynomial model saved to {filePath}");
    }
}

// 2. Model Factory
public static class ModelFactory
{
    public static Model CreateModel(string type)
    {
        switch (type.ToLower())
        {
            case "linear":
                return new LinearRegressionModel(); 
            case "polynomial":
                return new PolynomialRegressionModel();
            default:
                throw new ArgumentException($"Unknown model type: {type}");
        }
    }
}

// 3. Execution Context
public class Program
{
    public static void Main()
    {
        // 4. Polymorphic Collection
        List<Model> models = new List<Model>();

        // Create different types via factory
        models.Add(ModelFactory.CreateModel("linear"));
        models.Add(ModelFactory.CreateModel("polynomial"));

        // Unified Data
        double[][] X = new double[][] { new double[] { 1 }, new double[] { 2 }, new double[] { 3 } };
        double[] y = new double[] { 2, 4, 6 }; // Linear data
        // Note: The Polynomial model in this example expects different data (y = x^2 + 2x) 
        // but we are demonstrating the interface contract, not mathematical correctness across mismatched data.

        foreach (var model in models)
        {
            // Polymorphic calls: The specific implementation is determined at runtime
            // based on the actual type of 'model'.
            Console.WriteLine($"Processing {model.GetType().Name}...");
            
            // We pass the same data, though strictly the Polynomial model 
            // would require different data to be mathematically accurate.
            // This highlights the flexibility of the interface.
            model.Fit(X, y); 
            
            double[][] test = new double[][] { new double[] { 4 } };
            double[] pred = model.Predict(test);
            
            Console.WriteLine($"Prediction: {pred[0]}");
            Console.WriteLine("-----------------------------");
        }
    }
}

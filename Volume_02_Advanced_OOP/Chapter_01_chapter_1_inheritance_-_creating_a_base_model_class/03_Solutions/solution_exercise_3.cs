
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

// 1. Modified Base Class
public abstract class Model
{
    // Abstract properties enforce implementation details in derived classes
    public abstract string ModelId { get; set; }
    public abstract string Status { get; protected set; }

    public abstract void Fit(double[][] X, double[] y);
    public abstract double[] Predict(double[][] X);
    public abstract void Save(string filePath);
}

// 2. Updated Concrete Class (LinearRegressionModel shown)
public class LinearRegressionModel : Model
{
    private double _intercept;
    private double _slope;
    
    // Implementing abstract properties
    public override string ModelId { get; set; } = Guid.NewGuid().ToString();
    public override string Status { get; protected set; } = "Untrained";

    public override void Fit(double[][] X, double[] y)
    {
        // Simplified fitting logic for context
        _intercept = 0; 
        _slope = 2;

        // 3. Update State
        Status = "Trained";
        Console.WriteLine($"Model {ModelId} fitted. Status: {Status}");
    }

    public override double[] Predict(double[][] X)
    {
        if (Status != "Trained")
            throw new InvalidOperationException("Model must be trained before prediction.");
        
        double[] predictions = new double[X.Length];
        for (int i = 0; i < X.Length; i++)
            predictions[i] = _intercept + _slope * X[i][0];
        
        return predictions;
    }

    public override void Save(string filePath)
    {
        // Implementation similar to previous exercises
        Console.WriteLine("Model saved.");
    }
}

// 4. Execution
public class Program
{
    public static void Main()
    {
        List<Model> models = new List<Model>
        {
            new LinearRegressionModel(),
            new LinearRegressionModel() // Creating two instances
        };

        // Check initial status
        foreach (var m in models)
        {
            Console.WriteLine($"Instance {m.ModelId} is currently {m.Status}");
        }

        // Fit models
        double[][] dummyX = new double[][] { new double[] { 1 } };
        double[] dummyY = new double[] { 1 };

        foreach (var m in models)
        {
            m.Fit(dummyX, dummyY);
        }

        // Verify state change
        Console.WriteLine("\nAfter fitting:");
        foreach (var m in models)
        {
            Console.WriteLine($"Instance {m.ModelId} is now {m.Status}");
        }
    }
}


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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;

// 1. Base Class with Validation Logic
public abstract class Model
{
    public abstract string ModelId { get; set; }
    public abstract string Status { get; protected set; }

    // 2. Protected Validation Helper
    protected void ValidateData(double[][] X, double[] y)
    {
        if (X == null || y == null)
            throw new ArgumentNullException("Input data (X or y) cannot be null.");

        if (X.Length != y.Length)
            throw new ArgumentException($"Sample count mismatch: X has {X.Length} rows, y has {y.Length} rows.");

        for (int i = 0; i < X.Length; i++)
        {
            if (X[i] == null)
                throw new ArgumentException($"Row {i} of X is null.");

            // Check for NaN or Infinity in features
            foreach (double val in X[i])
            {
                if (double.IsNaN(val) || double.IsInfinity(val))
                    throw new ArgumentException($"Invalid value detected in X at row {i}: {val}");
            }
        }

        // Check for NaN or Infinity in targets
        foreach (double val in y)
        {
            if (double.IsNaN(val) || double.IsInfinity(val))
                throw new ArgumentException($"Invalid value detected in y: {val}");
        }
    }

    public abstract void Fit(double[][] X, double[] y);
    public abstract double[] Predict(double[][] X);
    public abstract void Save(string filePath);
}

// 3. Standard Model (Compliant)
public class CompliantLinearModel : Model
{
    public override string ModelId { get; set; } = "CompliantLinear";
    public override string Status { get; protected set; } = "Untrained";

    public override void Fit(double[][] X, double[] y)
    {
        // 4. Enforcing the contract
        ValidateData(X, y); 
        
        Console.WriteLine("Data validated. Fitting model...");
        // ... fitting logic ...
        Status = "Trained";
    }

    public override double[] Predict(double[][] X) { /* ... */ return new double[X.Length]; }
    public override void Save(string filePath) { /* ... */ }
}

// 5. Non-Compliant Model (Ignoring the contract)
public class CorruptModel : Model
{
    public override string ModelId { get; set; } = "CorruptModel";
    public override string Status { get; protected set; } = "Untrained";

    public override void Fit(double[][] X, double[] y)
    {
        // INTENTIONAL ERROR: Not calling ValidateData(X, y)
        // This model assumes inputs are always clean, leading to potential runtime crashes.
        Console.WriteLine("Skipping validation for speed (Bad Practice!)...");
        
        // If X is null or has mismatched length, this code will crash later.
        // Example unsafe operation:
        try 
        {
            double unsafeCalc = X[0][0] / y[0]; 
            Status = "Trained";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Crash in CorruptModel: {ex.Message}");
        }
    }

    public override double[] Predict(double[][] X) { return new double[X.Length]; }
    public override void Save(string filePath) { /* ... */ }
}

public class Program
{
    public static void Main()
    {
        // Valid Data
        double[][] validX = new double[][] { new double[] { 1.0 } };
        double[] validY = new double[] { 2.0 };

        // Invalid Data (Mismatched length)
        double[][] invalidX = new double[][] { new double[] { 1.0 }, new double[] { 2.0 } };
        double[] invalidY = new double[] { 5.0 };

        // 6. Demonstration
        var compliant = new CompliantLinearModel();
        var corrupt = new CorruptModel();

        Console.WriteLine("--- Testing Compliant Model ---");
        try
        {
            // This will succeed
            compliant.Fit(validX, validY);
            
            // This will fail gracefully with our exception
            compliant.Fit(invalidX, invalidY);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught expected error: {ex.Message}");
        }

        Console.WriteLine("\n--- Testing Corrupt Model ---");
        try
        {
            // This might crash unpredictably or behave unexpectedly
            // because it lacks the base class validation shield.
            corrupt.Fit(invalidX, invalidY);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught error (if handled): {ex.Message}");
        }
    }
}

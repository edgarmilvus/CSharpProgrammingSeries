
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.IO;

// 1. Abstract Base Class Definition
public abstract class Model
{
    public abstract void Fit(double[][] X, double[] y);
    public abstract double[] Predict(double[][] X);
    public abstract void Save(string filePath);
}

// 2. Concrete Implementation
public class LinearRegressionModel : Model
{
    // Private fields to store the learned parameters
    private double _intercept;
    private double _slope;

    // 3. Fit Implementation (Least Squares for Simple Linear Regression)
    public override void Fit(double[][] X, double[] y)
    {
        if (X.Length != y.Length)
            throw new ArgumentException("X and y must have the same number of samples.");

        int n = X.Length;
        double sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;

        // Calculate necessary sums
        for (int i = 0; i < n; i++)
        {
            // Access the single feature value at X[i][0]
            double xVal = X[i][0];
            double yVal = y[i];

            sumX += xVal;
            sumY += yVal;
            sumXY += xVal * yVal;
            sumXX += xVal * xVal;
        }

        // Calculate slope (beta) and intercept (alpha)
        // Formula: beta = (n*sumXY - sumX*sumY) / (n*sumXX - sumX^2)
        // Formula: alpha = (sumY - beta*sumX) / n
        double denominator = n * sumXX - sumX * sumX;
        
        if (Math.Abs(denominator) < 1e-9)
            throw new InvalidOperationException("Cannot calculate regression: Input data is constant or collinear.");

        _slope = (n * sumXY - sumX * sumY) / denominator;
        _intercept = (sumY - _slope * sumX) / n;

        Console.WriteLine($"Model fitted: Intercept = {_intercept:F4}, Slope = {_slope:F4}");
    }

    // 4. Predict Implementation
    public override double[] Predict(double[][] X)
    {
        if (_slope == 0 && _intercept == 0)
            Console.WriteLine("Warning: Model has not been fitted yet. Predictions may be zero.");

        double[] predictions = new double[X.Length];
        for (int i = 0; i < X.Length; i++)
        {
            // y = alpha + beta * x
            predictions[i] = _intercept + _slope * X[i][0];
        }
        return predictions;
    }

    // 5. Save Implementation
    public override void Save(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Intercept: {_intercept}");
                writer.WriteLine($"Slope: {_slope}");
            }
            Console.WriteLine($"Model saved to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving model: {ex.Message}");
        }
    }
}

public class Program
{
    // 6. Main Method Execution
    public static void Main()
    {
        // Data: y = 2x
        double[][] X = new double[][] 
        { 
            new double[] { 1 }, 
            new double[] { 2 }, 
            new double[] { 3 } 
        };
        double[] y = new double[] { 2, 4, 6 };

        // Instantiate (Polymorphism in action)
        Model model = new LinearRegressionModel();

        // Fit
        model.Fit(X, y);

        // Predict
        double[][] testX = new double[][] { new double[] { 4 } };
        double[] predictions = model.Predict(testX);
        Console.WriteLine($"Prediction for x=4: {predictions[0]}"); // Should be close to 8

        // Save
        model.Save("model_params.txt");
    }
}


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

# Source File: theory_theoretical_foundations_part2.cs
# Description: Theoretical Foundations
# ==========================================

public class LinearRegressionModel : Model
{
    // Implementation specific to Linear Regression
    public override void Fit(double[] trainingData, double[] labels)
    {
        Console.WriteLine("Fitting Linear Regression model...");
        // Math logic for calculating slope and intercept
    }

    public override double Predict(double[] input)
    {
        Console.WriteLine("Predicting with Linear Regression...");
        return 0.0; // Placeholder for calculation
    }
}

public class NeuralNetworkModel : Model
{
    // Implementation specific to Neural Networks
    public override void Fit(double[] trainingData, double[] labels)
    {
        Console.WriteLine("Training Neural Network (Backpropagation)...");
        // Complex matrix operations
    }

    public override double Predict(double[] input)
    {
        Console.WriteLine("Predicting with Neural Network...");
        return 0.0; // Placeholder for forward pass
    }
}

// Usage in a generic pipeline
public class Pipeline
{
    public void RunInference(Model model, double[] data)
    {
        // The 'model' parameter accepts any derived type of Model
        // due to polymorphism.
        double result = model.Predict(data);
        
        // We can also call the shared method inherited from the base class
        model.Save("checkpoint.bin");
    }
}

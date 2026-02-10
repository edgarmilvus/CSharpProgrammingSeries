
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

public class Program
{
    public static void Main()
    {
        Neuron neuron = new Neuron();
        neuron.Bias = 0.5; // Set a bias value

        double[] inputs = { 1.0, 2.0, 3.0 };
        double[] weights = { 0.1, 0.2, 0.3 };

        double output = neuron.Compute(inputs, weights);
        
        Console.WriteLine($"Neuron Output: {output}");
    }
}

public class Neuron
{
    // Property for the bias term
    public double Bias { get; set; }

    public double Compute(double[] inputs, double[] weights)
    {
        // Safety check: arrays must be the same length
        if (inputs.Length != weights.Length)
        {
            Console.WriteLine("Error: Inputs and weights must be the same length.");
            return 0.0;
        }

        double weightedSum = 0.0;

        // Calculate weighted sum using a for loop
        for (int i = 0; i < inputs.Length; i++)
        {
            weightedSum += inputs[i] * weights[i];
        }

        // Add the bias to the total input
        double totalInput = weightedSum + Bias;

        // Apply the sigmoid activation function
        return CalculateSigmoid(totalInput);
    }

    // Helper method for the sigmoid function
    private double CalculateSigmoid(double input)
    {
        double e = 2.71828;
        return 1.0 / (1.0 + Math.Pow(e, -input));
    }
}

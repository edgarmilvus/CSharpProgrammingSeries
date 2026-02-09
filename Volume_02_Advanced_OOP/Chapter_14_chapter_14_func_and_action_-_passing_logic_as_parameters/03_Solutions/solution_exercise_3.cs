
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;

public class Neuron
{
    // The delegate holds the mathematical logic
    private readonly Func<double, double> _activation;

    public Neuron(Func<double, double> activation)
    {
        _activation = activation;
    }

    public double Fire(double input)
    {
        // Apply the injected logic to the input
        return _activation(input);
    }
}

public class Program
{
    public static void Main()
    {
        // 1. Define Activation Functions (Lambdas)
        
        // ReLU: Rectified Linear Unit (outputs 0 if negative, else the input)
        Func<double, double> reluLogic = x => x > 0 ? x : 0;

        // Sigmoid: S-shaped curve (squashes input between 0 and 1)
        Func<double, double> sigmoidLogic = x => 1.0 / (1.0 + Math.Exp(-x));

        // Tanh: Hyperbolic Tangent (squashes input between -1 and 1)
        Func<double, double> tanhLogic = x => Math.Tanh(x);

        // 2. Instantiate Neurons with specific behaviors
        Neuron reluNeuron = new Neuron(reluLogic);
        Neuron sigmoidNeuron = new Neuron(sigmoidLogic);
        Neuron tanhNeuron = new Neuron(tanhLogic);

        // 3. Test inputs
        double[] inputs = { 5.0, -5.0 };

        Console.WriteLine($"{"Input",-10} | {"ReLU",-15} | {"Sigmoid",-15} | {"Tanh",-15}");
        Console.WriteLine(new string('-', 65));

        foreach (var input in inputs)
        {
            double r = reluNeuron.Fire(input);
            double s = sigmoidNeuron.Fire(input);
            double t = tanhNeuron.Fire(input);

            Console.WriteLine($"{input,-10} | {r,-15:F4} | {s,-15:F4} | {t,-15:F4}");
        }
    }
}

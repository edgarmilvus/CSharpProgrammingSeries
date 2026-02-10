
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;

namespace AI_Data_Structures
{
    // ---------------------------------------------------------
    // 1. The Abstract Base Class (The Contract)
    // ---------------------------------------------------------
    // We use 'abstract' because we never want to create a generic "Model".
    // We only care about specific implementations (Linear, Polynomial, etc.).
    // This class defines the shared interface that all AI components must adhere to.
    public abstract class Model
    {
        // 'abstract' methods have no implementation here.
        // They force any non-abstract child class to provide the logic.
        
        /// <summary>
        /// Trains the model on the provided dataset.
        /// </summary>
        /// <param name="x">The input features (e.g., square footage).</param>
        /// <param name="y">The target values (e.g., house prices).</param>
        public abstract void Fit(double[] x, double[] y);

        /// <summary>
        /// Predicts a value based on input features.
        /// </summary>
        /// <param name="input">The input features to predict on.</param>
        /// <returns>The predicted value.</returns>
        public abstract double Predict(double input);

        /// <summary>
        /// Saves the model to a file (common functionality).
        /// </summary>
        public virtual void Save()
        {
            // 'virtual' allows this method to be overridden, but provides a default implementation.
            // Here, we simulate saving by printing to console.
            Console.WriteLine("Model state saved to disk.");
        }
    }

    // ---------------------------------------------------------
    // 2. Concrete Implementation: Linear Regression
    // ---------------------------------------------------------
    // This class inherits from Model and provides the specific mathematical logic.
    public class LinearRegressionModel : Model
    {
        // Fields to store the learned parameters (weights)
        private double _slope;
        private double _intercept;

        /// <summary>
        /// Implements the abstract Fit method using the Least Squares method.
        /// </summary>
        public override void Fit(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("Input and output arrays must be the same length.");
            }

            // Simple calculation of slope (m) and intercept (b) for y = mx + b
            // Note: In a real system, this would be a complex optimization algorithm.
            double sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXY += x[i] * y[i];
                sumXX += x[i] * x[i];
            }

            _slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
            _intercept = (sumY - _slope * sumX) / n;

            Console.WriteLine($"Linear Model Trained: y = {_slope:F2}x + {_intercept:F2}");
        }

        /// <summary>
        /// Implements the abstract Predict method.
        /// </summary>
        public override double Predict(double input)
        {
            return (_slope * input) + _intercept;
        }
    }

    // ---------------------------------------------------------
    // 3. Concrete Implementation: Polynomial Regression (2nd Degree)
    // ---------------------------------------------------------
    // This demonstrates polymorphism: a different implementation of the same interface.
    public class PolynomialRegressionModel : Model
    {
        private double _a; // x^2 coefficient
        private double _b; // x coefficient
        private double _c; // constant

        /// <summary>
        /// Fits a polynomial curve y = ax^2 + bx + c.
        /// </summary>
        public override void Fit(double[] x, double[] y)
        {
            // Simplified logic for demonstration purposes.
            // Real polynomial fitting requires matrix operations (e.g., Normal Equation).
            // We will hardcode values to simulate a trained state for this example.
            _a = 0.1;
            _b = 2.0;
            _c = 10.0;
            
            Console.WriteLine($"Polynomial Model Trained: y = {_a:F2}x^2 + {_b:F2}x + {_c:F2}");
        }

        /// <summary>
        /// Predicts using the polynomial formula.
        /// </summary>
        public override double Predict(double input)
        {
            return (_a * input * input) + (_b * input) + _c;
        }
    }

    // ---------------------------------------------------------
    // 4. The System Context (Usage)
    // ---------------------------------------------------------
    public class HousingPriceSystem
    {
        public static void RunDemo()
        {
            // Real-world data: Square footage (x) and Price in thousands (y)
            double[] squareFootage = { 1000, 1500, 2000, 2500 };
            double[] prices = { 150, 220, 280, 350 };

            Console.WriteLine("--- Scenario: Predicting House Prices ---\n");

            // -----------------------------------------------------
            // Using the Base Class Type (Polymorphism in Action)
            // -----------------------------------------------------
            // We can declare variables of the base type 'Model'.
            // This allows us to swap algorithms without changing the calling code.
            
            Model currentModel = new LinearRegressionModel();
            
            Console.WriteLine("1. Training Linear Model...");
            currentModel.Fit(squareFootage, prices);
            
            double predictionLinear = currentModel.Predict(1800);
            Console.WriteLine($"Prediction for 1800 sqft: ${predictionLinear:F2}k\n");

            // -----------------------------------------------------
            // Swapping the Implementation
            // -----------------------------------------------------
            // Notice the code below is identical to the block above, 
            // yet it executes completely different logic.
            
            Console.WriteLine("2. Switching to Polynomial Model...");
            currentModel = new PolynomialRegressionModel(); 
            
            currentModel.Fit(squareFootage, prices);
            
            double predictionPoly = currentModel.Predict(1800);
            Console.WriteLine($"Prediction for 1800 sqft: ${predictionPoly:F2}k\n");

            // -----------------------------------------------------
            // Using Common Functionality
            // -----------------------------------------------------
            // The 'Save' method is inherited from the base class.
            // We can call it on any Model, regardless of its specific type.
            Console.WriteLine("3. Saving Model State...");
            currentModel.Save();
        }
    }
}

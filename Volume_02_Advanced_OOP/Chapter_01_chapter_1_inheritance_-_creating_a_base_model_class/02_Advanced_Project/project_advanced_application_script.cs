
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;

// ---------------------------------------------------------
// 1. Abstract Base Class (The "Model" Contract)
// ---------------------------------------------------------
// We define an abstract base class. This acts as a blueprint.
// It forces any subclass to implement specific methods.
// We use 'abstract' because a generic "Model" doesn't exist; 
// we only ever use specific implementations (like a Regression model or a Classifier).
public abstract class Model
{
    // Abstract methods: No implementation here. 
    // The child class MUST provide the logic.
    public abstract void Fit(double[] inputs, double[] targets);
    public abstract double Predict(double input);
    
    // Virtual method: Provides a default implementation, 
    // but child classes can override it if needed.
    public virtual void Save(string path)
    {
        Console.WriteLine($"Saving model to {path}...");
        // Simulate serialization logic here.
        Console.WriteLine("Save complete.");
    }
}

// ---------------------------------------------------------
// 2. Concrete Implementation: ConservativeModel
// ---------------------------------------------------------
// This model recommends Bonds. It uses a simple linear formula.
// It inherits from 'Model' and MUST implement the abstract methods.
public class ConservativeModel : Model
{
    private double _interestRateSensitivity;

    public ConservativeModel()
    {
        // Initialize specific parameters for this strategy.
        _interestRateSensitivity = 0.5;
    }

    // 'override' keyword is used to implement the abstract method defined in the base class.
    public override void Fit(double[] inputs, double[] targets)
    {
        // In a real scenario, this would calculate weights using math.
        // Here, we simulate training by calculating an average sensitivity.
        double sum = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            sum += inputs[i];
        }
        _interestRateSensitivity = sum / inputs.Length;
        Console.WriteLine($"Conservative Model fitted. Sensitivity: {_interestRateSensitivity}");
    }

    public override double Predict(double input)
    {
        // Logic: If market stability (input) is high, recommendation score is high.
        // Formula: Score = Input * Sensitivity
        return input * _interestRateSensitivity;
    }
}

// ---------------------------------------------------------
// 3. Concrete Implementation: AggressiveModel
// ---------------------------------------------------------
// This model recommends Tech Stocks. It uses a non-linear formula.
public class AggressiveModel : Model
{
    private double _volatilityFactor;

    public AggressiveModel()
    {
        _volatilityFactor = 2.5;
    }

    public override void Fit(double[] inputs, double[] targets)
    {
        // Simulating a more complex training process.
        // We look for the maximum potential growth in the dataset.
        double maxInput = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] > maxInput)
            {
                maxInput = inputs[i];
            }
        }
        _volatilityFactor = maxInput;
        Console.WriteLine($"Aggressive Model fitted. Volatility Factor: {_volatilityFactor}");
    }

    public override double Predict(double input)
    {
        // Logic: High volatility (input) leads to exponential returns.
        // Formula: Score = Input^2 * VolatilityFactor
        return (input * input) * _volatilityFactor;
    }
}

// ---------------------------------------------------------
// 4. The System: Financial Advisor
// ---------------------------------------------------------
// This class demonstrates Polymorphism.
// It accepts the BASE class type 'Model', not the specific child classes.
// This allows it to work with ANY model strategy passed to it.
public class FinancialAdvisor
{
    private Model _recommendationEngine;

    // Constructor injection: We pass the model strategy into the advisor.
    public FinancialAdvisor(Model strategy)
    {
        _recommendationEngine = strategy;
    }

    public void GenerateAdvice(double marketCondition)
    {
        Console.WriteLine("\n--- Starting Financial Analysis ---");
        
        // 1. Use the model to predict based on current market data.
        double score = _recommendationEngine.Predict(marketCondition);
        
        Console.WriteLine($"Market Condition Input: {marketCondition}");
        Console.WriteLine($"Recommendation Score: {score}");

        // 2. Interpret the score (Business Logic).
        if (score > 50)
        {
            Console.WriteLine("Advice: STRONG BUY");
        }
        else if (score > 20)
        {
            Console.WriteLine("Advice: HOLD");
        }
        else
        {
            Console.WriteLine("Advice: SELL");
        }

        // 3. Save the analysis record using the base class method.
        _recommendationEngine.Save("C:\\Finance\\Reports\\DailyAdvice.log");
    }
}

// ---------------------------------------------------------
// 5. Main Execution Block
// ---------------------------------------------------------
public class Program
{
    public static void Main()
    {
        // --- Step 1: Prepare Training Data ---
        // Simulating historical data (e.g., interest rates, volatility indices).
        double[] historicalInputs = { 10.0, 20.0, 30.0, 40.0 };
        double[] historicalTargets = { 1.0, 2.0, 3.0, 4.0 }; // Dummy targets

        // --- Step 2: Create Specific Model Instances ---
        // We instantiate the concrete classes.
        Model conservativeStrategy = new ConservativeModel();
        Model aggressiveStrategy = new AggressiveModel();

        // --- Step 3: Train the Models ---
        // Notice we call Fit on both, even though they calculate differently.
        conservativeStrategy.Fit(historicalInputs, historicalTargets);
        aggressiveStrategy.Fit(historicalInputs, historicalTargets);

        // --- Step 4: Deploy the Advisor (Polymorphism in Action) ---
        // We create an advisor using the Conservative strategy.
        FinancialAdvisor advisor1 = new FinancialAdvisor(conservativeStrategy);
        advisor1.GenerateAdvice(marketCondition: 85.0);

        // We create a second advisor using the Aggressive strategy.
        // The FinancialAdvisor class code did not change, but the behavior did.
        FinancialAdvisor advisor2 = new FinancialAdvisor(aggressiveStrategy);
        advisor2.GenerateAdvice(marketCondition: 85.0);
    }
}

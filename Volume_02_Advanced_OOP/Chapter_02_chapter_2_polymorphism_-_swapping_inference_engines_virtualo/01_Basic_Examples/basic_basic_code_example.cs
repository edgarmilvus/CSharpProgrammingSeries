
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

namespace SmartHomeSystem
{
    // 1. THE CONTRACT (Abstract Base Class / Interface)
    // We define what it means to be a "Temperature Control Engine".
    // It MUST provide a method to calculate the target temperature.
    // This acts as our "Tensor Operation" interface—a standard way to process inputs (sensor data)
    // into outputs (target temp).
    public abstract class TemperatureControlEngine
    {
        // 'abstract' enforces that derived classes MUST implement this method.
        // 'virtual' allows derived classes to override it if they want to provide
        // a specific implementation.
        public abstract double CalculateTargetTemp(double currentTemp, double desiredTemp);
    }

    // 2. CONCRETE IMPLEMENTATION A: The Neural Network Predictor
    // This simulates a complex AI model that predicts the best temperature
    // based on external factors (simulated here by simple math).
    public class NeuralNetPredictor : TemperatureControlEngine
    {
        // 'override' explicitly states that this method replaces the abstract definition
        // in the base class. The compiler checks that the signature matches exactly.
        public override double CalculateTargetTemp(double currentTemp, double desiredTemp)
        {
            // Simulating complex tensor operations/logic:
            // The AI might decide to overshoot slightly to save energy later.
            double predictedLoad = 1.5; // Simulated load factor
            double target = desiredTemp * predictedLoad - (currentTemp * 0.5);
            
            Console.WriteLine($"[AI Predictor] Calculated Target: {target}°C based on predictive models.");
            return target;
        }
    }

    // 3. CONCRETE IMPLEMENTATION B: The Rule-Based Expert System
    // This is a simpler, deterministic engine. It just tries to reach the desired temp.
    public class ExpertSystemThermostat : TemperatureControlEngine
    {
        public override double CalculateTargetTemp(double currentTemp, double desiredTemp)
        {
            // Logic: If we are far off, jump to desired. If close, hold steady.
            double difference = Math.Abs(currentTemp - desiredTemp);
            double target = (difference > 2.0) ? desiredTemp : currentTemp;

            Console.WriteLine($"[Expert System] Calculated Target: {target}°C based on strict rules.");
            return target;
        }
    }

    // 4. THE CLIENT CODE: The Smart Home Controller
    // This class does NOT know which engine it is using. 
    // It only knows it has a 'TemperatureControlEngine'.
    public class SmartHomeController
    {
        private TemperatureControlEngine _engine;

        // Constructor Injection: We pass the engine in.
        // This allows us to swap engines easily.
        public SmartHomeController(TemperatureControlEngine engine)
        {
            _engine = engine;
        }

        public void RunCycle(double current, double desired)
        {
            Console.WriteLine("--- Controller Cycle Start ---");
            
            // POLYMORPHISM IN ACTION:
            // The code below calls 'CalculateTargetTemp'. 
            // At runtime, the CLR looks at the actual object type stored in _engine
            // (either NeuralNetPredictor or ExpertSystemThermostat) and executes that specific version.
            double result = _engine.CalculateTargetTemp(current, desired);
            
            Console.WriteLine($"Setting HVAC to: {result}°C");
            Console.WriteLine("--- Controller Cycle End ---\n");
        }

        // Method to swap engines dynamically
        public void SwapEngine(TemperatureControlEngine newEngine)
        {
            Console.WriteLine(">>> Swapping Inference Engine...");
            _engine = newEngine;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // Scenario 1: Daytime, we use the AI Predictor for efficiency
            var aiEngine = new NeuralNetPredictor();
            var controller = new SmartHomeController(aiEngine);

            controller.RunCycle(current: 22.0, desired: 24.0);

            // Scenario 2: Nighttime, we swap to the reliable Expert System
            // Notice the 'RunCycle' method is called exactly the same way.
            var ruleEngine = new ExpertSystemThermostat();
            controller.SwapEngine(ruleEngine);

            controller.RunCycle(current: 22.0, desired: 24.0);
        }
    }
}

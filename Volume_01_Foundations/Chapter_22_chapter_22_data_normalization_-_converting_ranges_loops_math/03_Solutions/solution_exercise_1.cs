
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

class SensorConverter
{
    static void Main()
    {
        // Define the known ranges
        double inputMin = 0;
        double inputMax = 1023;
        double outputMin = -10; // Celsius
        double outputMax = 50;

        // 1. Ask user for input
        Console.Write("Enter sensor value (0-1023): ");
        string userInput = Console.ReadLine();
        
        // 2. Convert string input to a double
        double sensorValue = double.Parse(userInput);

        // 3. Calculate the normalized position (0.0 to 1.0)
        // Formula: (Input - InputMin) / (InputMax - InputMin)
        double normalizedPosition = (sensorValue - inputMin) / (inputMax - inputMin);

        // 4. Scale to the target range
        // Formula: NormalizedPosition * (OutputMax - OutputMin) + OutputMin
        double temperature = normalizedPosition * (outputMax - outputMin) + outputMin;

        // 5. Round to 2 decimal places for display
        double roundedTemp = Math.Round(temperature, 2);

        // 6. Display the result
        Console.WriteLine($"Sensor value {sensorValue} corresponds to {roundedTemp}°C.");
    }
}

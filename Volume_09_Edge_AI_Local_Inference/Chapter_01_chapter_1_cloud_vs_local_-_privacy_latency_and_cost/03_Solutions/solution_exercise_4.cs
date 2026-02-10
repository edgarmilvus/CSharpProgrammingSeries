
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

namespace CostAnalysis
{
    public class CostInputs
    {
        // Cloud
        public double CostPer1KTokens { get; set; }
        public int MonthlyApiCalls { get; set; }
        public int AvgTokensPerCall { get; set; }

        // Local
        public double HardwareCost { get; set; } // One-time
        public double PowerConsumptionWatts { get; set; }
        public double ElectricityRatePerKWh { get; set; }
        public int HardwareLifespanYears { get; set; }
    }

    public class CostResult
    {
        public double TotalCloudCost { get; set; }
        public double TotalLocalCost { get; set; }
        public double LocalAmortizedCost { get; set; } // Cost per month
    }

    public class CostCalculator
    {
        public CostResult CalculateTCO(CostInputs inputs, int months)
        {
            // 1. Cloud Cost Calculation
            // Formula: (Calls * Tokens/Call / 1000) * CostPer1K
            double tokensPerMonth = inputs.MonthlyApiCalls * inputs.AvgTokensPerCall;
            double costPerMonthCloud = (tokensPerMonth / 1000.0) * inputs.CostPer1KTokens;
            double totalCloud = costPerMonthCloud * months;

            // 2. Local Cost Calculation
            // Power Cost: (Watts / 1000) * 24 hours * 30 days * Rate
            double monthlyPowerCost = (inputs.PowerConsumptionWatts / 1000.0) * 24 * 30 * inputs.ElectricityRatePerKWh;
            
            // Hardware Depreciation (Linear)
            double totalMonthsLifespan = inputs.HardwareLifespanYears * 12;
            double monthlyDepreciation = inputs.HardwareCost / totalMonthsLifespan;
            
            double totalLocal = (monthlyPowerCost + monthlyDepreciation) * months;

            // 3. Amortization (Shared Hardware Scenario)
            // Assume hardware is shared among 5 applications
            double sharedFactor = 5.0;
            double amortizedMonthly = (monthlyPowerCost + (monthlyDepreciation / sharedFactor));
            double totalAmortized = amortizedMonthly * months;

            return new CostResult
            {
                TotalCloudCost = totalCloud,
                TotalLocalCost = totalLocal,
                LocalAmortizedCost = totalAmortized
            };
        }

        public void PrintBreakEven(CostInputs inputs)
        {
            Console.WriteLine("\n--- Cost Analysis (12 Month Projection) ---");
            Console.WriteLine($"Cloud Cost/Month: ${CalculateTCO(inputs, 1).TotalCloudCost:F2}");
            Console.WriteLine($"Local Cost/Month: ${CalculateTCO(inputs, 1).TotalLocalCost:F2}");
            
            var result12 = CalculateTCO(inputs, 12);
            Console.WriteLine($"\nTotal 12-Month Cost (Cloud): ${result12.TotalCloudCost:F2}");
            Console.WriteLine($"Total 12-Month Cost (Local): ${result12.TotalLocalCost:F2}");
            Console.WriteLine($"Total 12-Month Cost (Local/Shared): ${result12.LocalAmortizedCost:F2}");

            // Break-even logic
            int monthsToBreakEven = -1;
            for (int m = 1; m <= inputs.HardwareLifespanYears * 12; m++)
            {
                var res = CalculateTCO(inputs, m);
                if (res.TotalLocalCost < res.TotalCloudCost)
                {
                    monthsToBreakEven = m;
                    break;
                }
            }

            if (monthsToBreakEven > 0)
            {
                Console.WriteLine($"\nBreak-even point: {monthsToBreakEven} months.");
            }
            else
            {
                Console.WriteLine("\nCloud remains cheaper than Local within the hardware lifespan.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var inputs = new CostInputs
            {
                // Cloud: $0.002 per 1K tokens
                CostPer1KTokens = 0.002,
                MonthlyApiCalls = 50000,
                AvgTokensPerCall = 500,

                // Local: RTX 3060 equivalent
                HardwareCost = 500.0,
                PowerConsumptionWatts = 170, // Under load
                ElectricityRatePerKWh = 0.15,
                HardwareLifespanYears = 3
            };

            var calculator = new CostCalculator();
            calculator.PrintBreakEven(inputs);
        }
    }
}

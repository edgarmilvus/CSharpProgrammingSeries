
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

// Source File: theory_theoretical_foundations_part2.cs
// Description: Theoretical Foundations
// ==========================================

public class AIPlanner
{
    // Returns an Option, explicitly acknowledging the possibility of failure
    public Option<ExecutionPlan> GeneratePlan(string prompt)
    {
        // Simulate AI inference
        string rawOutput = CallLLM(prompt);

        if (string.IsNullOrWhiteSpace(rawOutput))
        {
            return Option<ExecutionPlan>.None();
        }

        try
        {
            var plan = ParsePlan(rawOutput);
            return Option<ExecutionPlan>.Some(plan);
        }
        catch (FormatException)
        {
            // The AI hallucinated a format we don't understand
            return Option<ExecutionPlan>.None();
        }
    }

    private string CallLLM(string prompt) { /* ... */ return ""; }
    private ExecutionPlan ParsePlan(string raw) { /* ... */ return new ExecutionPlan(); }
}

public class AIExecutor
{
    public void Execute(Option<ExecutionPlan> planOption)
    {
        // We are forced to handle the None case explicitly
        if (!planOption.HasValue)
        {
            Console.WriteLine("No valid plan generated. Aborting execution.");
            return;
        }

        // Accessing .Value is safe here because we checked HasValue
        ExecutionPlan plan = planOption.Value;
        Console.WriteLine($"Executing plan: {plan.Name}");
    }
}

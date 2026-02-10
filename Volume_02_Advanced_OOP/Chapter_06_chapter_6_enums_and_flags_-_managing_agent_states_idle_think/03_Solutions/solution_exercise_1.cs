
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;

public enum AgentState
{
    Idle,
    Parsing,
    Inference,
    Error
}

public class TensorProcessingAgent
{
    public AgentState CurrentState { get; private set; }

    public TensorProcessingAgent()
    {
        CurrentState = AgentState.Idle;
    }

    public void Transition(AgentState nextState)
    {
        bool isValid = false;

        // Define the transition rules based on the current state and the target state
        if (CurrentState == AgentState.Idle && nextState == AgentState.Parsing) isValid = true;
        else if (CurrentState == AgentState.Parsing && nextState == AgentState.Inference) isValid = true;
        else if (CurrentState == AgentState.Inference && nextState == AgentState.Idle) isValid = true;
        else if (nextState == AgentState.Error) isValid = true; // Any state can go to Error
        else if (CurrentState == AgentState.Error && nextState == AgentState.Idle) isValid = true; // Reset

        if (isValid)
        {
            Console.WriteLine($"[State Change] {CurrentState} -> {nextState}");
            CurrentState = nextState;
        }
        else
        {
            throw new InvalidOperationException($"Cannot transition from {CurrentState} to {nextState}.");
        }
    }
}

// Test Harness
public class Program
{
    public static void Main()
    {
        TensorProcessingAgent agent = new TensorProcessingAgent();

        try
        {
            // Valid Workflow
            agent.Transition(AgentState.Parsing); // Idle -> Parsing
            agent.Transition(AgentState.Inference); // Parsing -> Inference
            agent.Transition(AgentState.Idle); // Inference -> Idle

            // Invalid Transition (Idle -> Inference is not allowed)
            agent.Transition(AgentState.Inference); 
            
            // This line won't be reached due to the exception above
            Console.WriteLine("Test completed successfully.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Exception caught: {ex.Message}");
        }
    }
}

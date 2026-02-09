
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
using System.Collections.Generic;

// Constraint ensures T is an Enum (struct and IConvertible)
public class StateManager<T> where T : struct, IConvertible
{
    public T CurrentState { get; private set; }
    private readonly Dictionary<T, List<T>> _allowedTransitions;

    public StateManager(T initialState, Dictionary<T, List<T>> transitionRules)
    {
        CurrentState = initialState;
        _allowedTransitions = transitionRules;
    }

    public bool TryTransition(T targetState)
    {
        // Check if the current state has any defined rules
        if (_allowedTransitions.ContainsKey(CurrentState))
        {
            List<T> validNextStates = _allowedTransitions[CurrentState];

            // Iterate manually to find a match (No LINQ allowed)
            for (int i = 0; i < validNextStates.Count; i++)
            {
                // Use Equals for comparison
                if (validNextStates[i].Equals(targetState))
                {
                    CurrentState = targetState;
                    return true; // Transition successful
                }
            }
        }
        return false; // Transition invalid
    }
}

// Specific Enum for testing
public enum TrafficLight { Red, Yellow, Green }

public class Program
{
    public static void Main()
    {
        // Define rules: Red -> Green, Green -> Yellow, Yellow -> Red
        var rules = new Dictionary<TrafficLight, List<TrafficLight>>
        {
            { TrafficLight.Red, new List<TrafficLight> { TrafficLight.Green } },
            { TrafficLight.Green, new List<TrafficLight> { TrafficLight.Yellow } },
            { TrafficLight.Yellow, new List<TrafficLight> { TrafficLight.Red } }
        };

        StateManager<TrafficLight> manager = new StateManager<TrafficLight>(TrafficLight.Red, rules);

        // Cycle 5 times
        for (int i = 1; i <= 5; i++)
        {
            TrafficLight next = GetNextLight(manager.CurrentState);
            
            if (manager.TryTransition(next))
            {
                Console.WriteLine($"Cycle {i}: Transitioned to {manager.CurrentState}");
            }
            else
            {
                Console.WriteLine($"Cycle {i}: Failed to transition to {next}");
            }
        }
    }

    // Helper to determine the logical next light for the test loop
    private static TrafficLight GetNextLight(TrafficLight current)
    {
        if (current == TrafficLight.Red) return TrafficLight.Green;
        if (current == TrafficLight.Green) return TrafficLight.Yellow;
        if (current == TrafficLight.Yellow) return TrafficLight.Red;
        return TrafficLight.Red;
    }
}

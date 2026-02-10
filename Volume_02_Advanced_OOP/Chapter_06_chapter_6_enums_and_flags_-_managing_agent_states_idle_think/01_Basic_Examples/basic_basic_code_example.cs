
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
    // Defines the strict, mutually exclusive states an agent can be in.
    // Enums are value types and inherently type-safe.
    public enum AgentState
    {
        Idle,       // Waiting for input
        Thinking,   // Processing data
        Error       // Critical failure
    }

    // Represents the AI agent controlling the home.
    public class SmartHomeAgent
    {
        // Property to hold the current state.
        // We use a private backing field to control access.
        private AgentState _currentState;

        public AgentState CurrentState
        {
            get { return _currentState; }
            private set { _currentState = value; }
        }

        // Constructor initializes the agent to Idle.
        public SmartHomeAgent()
        {
            CurrentState = AgentState.Idle;
            Console.WriteLine("Agent initialized. State: Idle");
        }

        // Attempts to transition to a new state.
        // This method encapsulates the business logic for state management.
        public void TransitionState(AgentState newState)
        {
            Console.WriteLine($"Attempting to transition from {CurrentState} to {newState}...");

            // Logic to validate state transitions.
            // This prevents invalid sequences, such as going from Error to Thinking directly.
            bool isValid = false;

            if (CurrentState == AgentState.Idle && newState == AgentState.Thinking)
            {
                isValid = true; // Idle -> Thinking is valid
            }
            else if (CurrentState == AgentState.Thinking && newState == AgentState.Idle)
            {
                isValid = true; // Thinking -> Idle is valid (task complete)
            }
            else if (CurrentState == AgentState.Thinking && newState == AgentState.Error)
            {
                isValid = true; // Thinking -> Error is valid (processing failed)
            }
            else if (CurrentState == AgentState.Error && newState == AgentState.Idle)
            {
                isValid = true; // Error -> Idle is valid (reset)
            }
            else if (CurrentState == newState)
            {
                isValid = true; // Staying in the same state is allowed
            }

            if (isValid)
            {
                CurrentState = newState;
                Console.WriteLine($"Success. New State: {CurrentState}");
            }
            else
            {
                Console.WriteLine($"Failed. Cannot transition from {CurrentState} to {newState}.");
            }
        }

        // Simulates processing a request.
        public void ProcessRequest(string request)
        {
            if (CurrentState != AgentState.Idle)
            {
                Console.WriteLine($"Cannot process '{request}'. Agent is currently {CurrentState}.");
                return;
            }

            TransitionState(AgentState.Thinking);

            // Simulate work
            Console.WriteLine($"Processing: {request}...");
            
            // Simulate a random failure during processing
            Random rnd = new Random();
            if (rnd.Next(0, 2) == 0) // 50% chance of error for demonstration
            {
                TransitionState(AgentState.Error);
            }
            else
            {
                TransitionState(AgentState.Idle);
            }
        }
    }

    // Main program to run the simulation.
    class Program
    {
        static void Main(string[] args)
        {
            SmartHomeAgent agent = new SmartHomeAgent();

            // 1. Start a valid request
            agent.ProcessRequest("Turn on living room lights");

            // 2. Demonstrate an invalid transition attempt
            // (Assuming the agent ended in Error state from the previous random chance)
            if (agent.CurrentState == AgentState.Error)
            {
                // Try to go straight to Thinking (Invalid)
                agent.TransitionState(AgentState.Thinking);
                
                // Correct way: Reset to Idle first
                agent.TransitionState(AgentState.Idle);
            }

            // 3. Process another request
            agent.ProcessRequest("Set thermostat to 72 degrees");
        }
    }
}

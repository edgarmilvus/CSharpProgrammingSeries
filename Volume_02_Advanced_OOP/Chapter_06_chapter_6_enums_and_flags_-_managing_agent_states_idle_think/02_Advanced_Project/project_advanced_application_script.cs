
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

namespace TensorProcessingSystem
{
    // 1. DEFINING THE PRIMARY STATE ENUM
    // We use an Enum to enforce mutually exclusive states. An agent can only be in ONE of these at a time.
    // This prevents logical contradictions (e.g., an agent being both Idle and in Error simultaneously).
    public enum AgentState
    {
        Idle,       // Ready to accept a new tensor
        Thinking,   // Currently processing data
        Error       // Critical failure, requires intervention
    }

    // 2. DEFINING THE FLAGS ENUM FOR COMPOSITE ATTRIBUTES
    // We use the [Flags] attribute to allow bitwise operations.
    // These represent transient attributes that can exist alongside the primary AgentState.
    [Flags]
    public enum AgentAttributes
    {
        None = 0,               // No special attributes
        AwaitingInput = 1,      // Waiting for external data (2^0)
        HighPriority = 2,       // Needs immediate attention (2^1)
        UsingGPU = 4,           // Consuming GPU resources (2^2)
        CacheValid = 8          // Local memory is up to date (2^3)
    }

    // 3. THE TENSOR CLASS
    // A simplified representation of data for our AI model.
    public class Tensor
    {
        public string Name { get; set; }
        public double[] Data { get; set; }

        public Tensor(string name, int size)
        {
            Name = name;
            Data = new double[size];
            // Initialize with dummy data
            for (int i = 0; i < size; i++)
            {
                Data[i] = 1.0;
            }
        }
    }

    // 4. THE STATE TRANSITION MANAGER
    // This class encapsulates the logic for managing state changes safely.
    public class AgentSupervisor
    {
        // Private fields to hold the current state and attributes
        private AgentState _currentState;
        private AgentAttributes _currentAttributes;

        public AgentSupervisor()
        {
            // Initial state is strictly Idle with no attributes
            _currentState = AgentState.Idle;
            _currentAttributes = AgentAttributes.None;
        }

        // Property to expose state safely
        public AgentState CurrentState => _currentState;

        // 5. STATE TRANSITION LOGIC
        // This method ensures we only move to valid states.
        public void TransitionState(AgentState newState)
        {
            // Logic: Prevent illegal transitions
            if (_currentState == AgentState.Error && newState != AgentState.Idle)
            {
                // In a real system, we might log this or throw a custom exception.
                // Here, we simply refuse the transition.
                Console.WriteLine($"[Safety Lock] Cannot transition from Error to {newState}. Reset required.");
                return;
            }

            // Logic: Prevent redundant transitions
            if (_currentState == newState)
            {
                Console.WriteLine($"[Info] Agent is already in state: {newState}.");
                return;
            }

            Console.WriteLine($"[Transition] {_currentState} -> {newState}");
            _currentState = newState;
        }

        // 6. ATTRIBUTE MANAGEMENT (FLAGS)
        // Methods to add or remove specific flags using bitwise operators.
        public void AddAttribute(AgentAttributes attribute)
        {
            // The OR operator (|) adds a flag without affecting others.
            _currentAttributes |= attribute;
            Console.WriteLine($"[Attribute Added] {attribute}. Current Flags: {_currentAttributes}");
        }

        public void RemoveAttribute(AgentAttributes attribute)
        {
            // The AND (&) combined with NOT (~) removes a specific flag.
            _currentAttributes &= ~attribute;
            Console.WriteLine($"[Attribute Removed] {attribute}. Current Flags: {_currentAttributes}");
        }

        // 7. COMPOSITE CHECK LOGIC
        // Checking if specific flags are set using bitwise AND.
        public bool HasAttribute(AgentAttributes attribute)
        {
            // If (_currentAttributes & attribute) == attribute, the bit is set.
            return (_currentAttributes & attribute) == attribute;
        }

        // 8. PROCESSING SIMULATION
        public void ProcessTensor(Tensor tensor)
        {
            if (_currentState != AgentState.Idle)
            {
                Console.WriteLine($"[Error] Cannot process {tensor.Name}. Agent is busy or in error state.");
                return;
            }

            // Start Processing
            TransitionState(AgentState.Thinking);
            AddAttribute(AgentAttributes.UsingGPU);

            // Simulate work
            Console.WriteLine($"Processing tensor: {tensor.Name}...");

            // Simulate a complex scenario: The model requires external data
            if (tensor.Data.Length > 5)
            {
                Console.WriteLine(">> Model requires additional context.");
                AddAttribute(AgentAttributes.AwaitingInput);
                
                // Check composite state
                if (HasAttribute(AgentAttributes.AwaitingInput) && _currentState == AgentState.Thinking)
                {
                    Console.WriteLine(">> [System Alert] Active processing paused. Waiting for user.");
                }
            }

            // Simulate completion
            RemoveAttribute(AgentAttributes.AwaitingInput);
            RemoveAttribute(AgentAttributes.UsingGPU);
            TransitionState(AgentState.Idle);
        }

        // 9. ERROR HANDLING SIMULATION
        public void TriggerError()
        {
            // Force state to Error
            _currentState = AgentState.Error;
            // Clear attributes that are no longer valid during a crash
            _currentAttributes = AgentAttributes.None; 
            Console.WriteLine(">> [CRITICAL] System Failure detected. State set to Error.");
        }

        // 10. RECOVERY MECHANISM
        public void ResetSystem()
        {
            if (_currentState == AgentState.Error)
            {
                TransitionState(AgentState.Idle);
                Console.WriteLine(">> [Recovery] System reset complete.");
            }
        }
    }

    // 11. MAIN EXECUTION BLOCK
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Starting Tensor Processing Supervisor ---\n");

            // Initialize the supervisor
            AgentSupervisor supervisor = new AgentSupervisor();

            // Create Tensors
            Tensor smallTensor = new Tensor("Image_Batch_1", 3);
            Tensor largeTensor = new Tensor("Video_Stream_4k", 10);

            // Scenario 1: Standard Processing
            Console.WriteLine("\n--- SCENARIO 1: Standard Processing ---");
            supervisor.ProcessTensor(smallTensor);

            // Scenario 2: Complex Processing requiring input
            Console.WriteLine("\n--- SCENARIO 2: Complex Processing (Requires Input) ---");
            supervisor.ProcessTensor(largeTensor);

            // Scenario 3: Error Handling
            Console.WriteLine("\n--- SCENARIO 3: Simulating System Failure ---");
            supervisor.TriggerError();
            
            // Attempt illegal transition
            supervisor.ProcessTensor(smallTensor); 

            // Scenario 4: Recovery
            Console.WriteLine("\n--- SCENARIO 4: Recovery ---");
            supervisor.ResetSystem();
            
            // Verify state
            if (supervisor.CurrentState == AgentState.Idle)
            {
                Console.WriteLine(">> System verified: Ready for new tasks.");
            }

            Console.WriteLine("\n--- End of Simulation ---");
        }
    }
}


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

// 1. Define the Delegate for Simulation Updates
// We are introducing Delegates here. A delegate is a type that represents references 
// to methods with a particular parameter list and return type. 
// Here, we define a delegate named 'SimulationUpdate' that points to methods 
// taking a 'GpuMatrix' and returning nothing (void).
public delegate void SimulationUpdate(GpuMatrix matrix);

namespace AdvancedOOP
{
    // 2. The Unmanaged Resource Class (GpuMatrix)
    // This class simulates a heavy resource (like a GPU buffer).
    // It MUST implement IDisposable to signal that it holds unmanaged resources.
    public class GpuMatrix : IDisposable
    {
        // Pointer to unmanaged memory (Simulated with IntPtr)
        private IntPtr _vramHandle;
        private bool _disposed = false; // Flag to track if we've already cleaned up

        // Constructor: Allocates the "VRAM"
        public GpuMatrix(int rows, int cols)
        {
            // Simulate allocating a block of memory on the GPU
            // In a real scenario, this would be cudaMalloc or similar.
            _vramHandle = Marshal.AllocHGlobal(rows * cols * sizeof(float));
            Console.WriteLine($"[VRAM Allocated] Handle: {_vramHandle} for {rows}x{cols} Matrix.");
        }

        // 3. The Dispose Method (Deterministic Cleanup)
        // This is the core of manual resource management. 
        // It allows us to release resources immediately when we are done with them.
        public void Dispose()
        {
            // Call the internal cleanup method, passing 'true' to indicate 
            // we are calling it explicitly, not from the Garbage Collector.
            Dispose(true);
            
            // 4. Suppress Finalization
            // This is a critical optimization. Since we have manually cleaned up 
            // the resources, we tell the GC not to bother running the Finalizer 
            // on this object later. This improves performance.
            GC.SuppressFinalize(this);
        }

        // 5. Protected Virtual Dispose Method (The Pattern)
        // This method contains the actual logic for cleaning up resources.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return; // Safety check: Don't clean up twice!

            if (disposing)
            {
                // Cleanup Managed Resources
                // If this class held references to other .NET objects (like streams),
                // we would dispose them here.
                // e.g., _someOtherStream?.Dispose();
            }

            // Cleanup Unmanaged Resources
            // This block ALWAYS runs if we are cleaning up.
            if (_vramHandle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_vramHandle); // Release the simulated VRAM
                _vramHandle = IntPtr.Zero;
                Console.WriteLine($"[VRAM Released] Handle: {_vramHandle}.");
            }

            _disposed = true;
        }

        // 6. The Finalizer (The Safety Net)
        // This is the "Emergency Brake". If the programmer forgets to call Dispose(),
        // the Garbage Collector will eventually call this method.
        // It ensures resources aren't leaked forever, but it is non-deterministic.
        ~GpuMatrix()
        {
            // We pass 'false' here because we are being called by the GC.
            // We must NOT touch managed members (other C# objects) because 
            // they might have already been garbage collected.
            Dispose(false);
        }

        public void PerformCalculation()
        {
            if (_disposed) throw new ObjectDisposedException("GpuMatrix");
            Console.WriteLine("...Performing heavy matrix multiplication on GPU...");
        }
    }

    // 7. The Simulation Controller
    // This class orchestrates the usage of the GpuMatrix.
    public class FluidSimulation
    {
        // The Delegate instance. We hook up a Lambda Expression to it later.
        private SimulationUpdate _onUpdate;

        public FluidSimulation()
        {
            // Initialize with a default empty delegate to avoid null reference errors
            _onUpdate = (matrix) => { };
        }

        // Method to register a callback using a Lambda Expression
        public void RegisterCallback(SimulationUpdate callback)
        {
            _onUpdate += callback;
        }

        public void RunSimulationStep()
        {
            Console.WriteLine("\n--- Starting Simulation Step ---");

            // 8. The 'using' Statement (The Best Practice)
            // This is syntactic sugar. It compiles into a try/finally block.
            // It guarantees that 'matrix.Dispose()' is called when the block exits,
            // whether the block finishes normally or throws an exception.
            using (GpuMatrix matrix = new GpuMatrix(1024, 1024))
            {
                try
                {
                    // Perform operations
                    matrix.PerformCalculation();

                    // Invoke the delegate (Lambda Expression) passed by the user
                    // This allows external code to interact with the matrix safely.
                    _onUpdate(matrix);

                    // Simulate a crash or complex logic path
                    // Uncomment the line below to see how 'using' handles exceptions:
                    // throw new InvalidOperationException("Simulation failed!"); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Caught exception: {ex.Message}");
                    // Even if we crash here, the 'using' block ensures Dispose() is called.
                }
            } // <-- matrix.Dispose() is called HERE automatically.

            Console.WriteLine("--- Simulation Step Ended ---");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 9. Setting up the System
            FluidSimulation sim = new FluidSimulation();

            // 10. Using a Lambda Expression to define behavior
            // We define a small anonymous function right here.
            // It captures the logic we want to run inside the simulation step.
            // This keeps the code clean and modular.
            sim.RegisterCallback((m) => 
            {
                Console.WriteLine("   [Lambda Callback] Monitoring GPU temperature...");
            });

            // 11. Running the Simulation
            // This demonstrates the full lifecycle: Allocation -> Usage -> Disposal
            sim.RunSimulationStep();

            Console.WriteLine("\n[System Status] Simulation finished. Resources cleaned up deterministically.");
        }
    }
}

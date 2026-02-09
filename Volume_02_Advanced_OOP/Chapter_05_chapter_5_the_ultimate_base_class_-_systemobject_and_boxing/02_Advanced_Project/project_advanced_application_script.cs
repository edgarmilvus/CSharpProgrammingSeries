
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

namespace AdvancedOOP_DataStructures
{
    // ---------------------------------------------------------
    // 1. The Abstract Base Class (System.Object derivative)
    // ---------------------------------------------------------
    // We define an abstract base class to represent any mathematical entity.
    // This serves as the "Ultimate Base Class" for our hierarchy, allowing
    // us to treat Scalars and Vectors polymorphically.
    public abstract class MathematicalEntity
    {
        // A common property for all entities (Name).
        public string Name { get; set; }

        public MathematicalEntity(string name)
        {
            this.Name = name;
        }

        // An abstract method that must be implemented by derived types.
        // This calculates the magnitude (norm) of the entity.
        public abstract double CalculateMagnitude();

        // A virtual method providing a default string representation.
        // Derived classes can override this if needed.
        public override string ToString()
        {
            return $"Entity: {Name}";
        }
    }

    // ---------------------------------------------------------
    // 2. Value Type for High-Performance Data (Struct)
    // ---------------------------------------------------------
    // We use a 'struct' (value type) to hold raw numerical data.
    // Unlike 'class', 'struct' instances are stored on the stack (usually),
    // avoiding heap allocation and garbage collection overhead.
    // Crucially, this struct does NOT inherit from MathematicalEntity,
    // allowing it to remain a lightweight data container.
    public struct DataPoint
    {
        public double X;
        public double Y;
        public double Z;

        public DataPoint(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    // ---------------------------------------------------------
    // 3. Tensor-Like Structure (Composition over Inheritance)
    // ---------------------------------------------------------
    // This class represents a Tensor. It uses an array of 'DataPoint' structs.
    // Because 'DataPoint' is a value type, the array stores the raw data
    // directly in contiguous memory, not references to boxed objects.
    public class Tensor3D : MathematicalEntity
    {
        // Internal storage: An array of structs.
        // No boxing occurs here.
        private DataPoint[] _dataPoints;
        private int _count;

        public Tensor3D(string name, int capacity) : base(name)
        {
            _dataPoints = new DataPoint[capacity];
            _count = 0;
        }

        // Method to add data.
        // We pass the struct by value. No boxing.
        public void AddDataPoint(double x, double y, double z)
        {
            if (_count < _dataPoints.Length)
            {
                _dataPoints[_count] = new DataPoint(x, y, z);
                _count++;
            }
        }

        // Override the abstract method from the base class.
        // This computes the Euclidean norm of the tensor (simplified).
        public override double CalculateMagnitude()
        {
            double sumSquares = 0.0;
            
            // Iterating through the array of structs.
            // Accessing .X, .Y, .Z is direct memory access.
            for (int i = 0; i < _count; i++)
            {
                DataPoint dp = _dataPoints[i]; // Copy struct (cheap)
                sumSquares += (dp.X * dp.X) + (dp.Y * dp.Y) + (dp.Z * dp.Z);
            }

            return Math.Sqrt(sumSquares);
        }

        // Override ToString for detailed output.
        public override string ToString()
        {
            return $"Tensor3D '{Name}' containing {_count} data points.";
        }
    }

    // ---------------------------------------------------------
    // 4. Scalar Entity (Inherits from Base)
    // ---------------------------------------------------------
    // A simple scalar value represented as a class.
    // This is useful for comparison with the Tensor approach.
    public class ScalarValue : MathematicalEntity
    {
        // Note: To store the actual number, we use a 'double'.
        // Since 'double' is a value type, it is embedded directly
        // in the ScalarValue class instance on the heap.
        public double Value { get; set; }

        public ScalarValue(string name, double value) : base(name)
        {
            this.Value = value;
        }

        // Implementation of the abstract method.
        public override double CalculateMagnitude()
        {
            // Magnitude of a scalar is its absolute value.
            return Math.Abs(Value);
        }

        public override string ToString()
        {
            return $"Scalar '{Name}': {Value}";
        }
    }

    // ---------------------------------------------------------
    // 5. The Simulation Engine
    // ---------------------------------------------------------
    public class SimulationSystem
    {
        public static void Main()
        {
            Console.WriteLine("=== Advanced OOP: High-Performance Tensor Simulation ===");
            Console.WriteLine("Objective: Process particle data without Boxing overhead.\n");

            // 1. Create a Tensor instance.
            // This allocates memory for the array of structs on the heap,
            // but the structs themselves are packed tightly inside the array.
            Tensor3D particleSystem = new Tensor3D("ProtonCloud", 3);

            // 2. Add raw data.
            // 'DataPoint' is created on the stack, then copied into the array.
            // No heap allocation for the data point itself.
            particleSystem.AddDataPoint(1.5, 2.0, 3.5);
            particleSystem.AddDataPoint(0.5, 1.0, 0.5);
            particleSystem.AddDataPoint(4.0, 4.0, 4.0);

            // 3. Create a Scalar value.
            ScalarValue constantK = new ScalarValue("CouplingConstant", 9.81);

            // 4. Polymorphic Array (Object Array).
            // Here is the danger zone. We store both in an object[].
            // The 'ScalarValue' is a reference type, so no boxing occurs for the reference.
            // However, if we tried to store a raw 'double' here, it WOULD box.
            MathematicalEntity[] entities = new MathematicalEntity[2];
            entities[0] = particleSystem;
            entities[1] = constantK;

            // 5. Process the collection polymorphically.
            // We iterate using standard for-loop (allowed).
            // We cast or access methods via the base class reference.
            Console.WriteLine("\nProcessing Entities:");
            for (int i = 0; i < entities.Length; i++)
            {
                MathematicalEntity entity = entities[i];
                
                // Calculate magnitude using the abstract method.
                double mag = entity.CalculateMagnitude();
                
                Console.WriteLine($"- {entity.ToString()} | Magnitude: {mag:F4}");
            }

            // 6. Demonstrate Value Type Behavior (Stack vs Heap)
            Console.WriteLine("\n--- Memory Layout Analysis ---");
            
            // Creating a struct directly on the stack.
            DataPoint localPoint = new DataPoint(10, 20, 30);
            
            // Creating a class instance on the heap.
            ScalarValue localScalar = new ScalarValue("Local", 100.0);

            Console.WriteLine($"Struct (Value Type) Data: X={localPoint.X}, Y={localPoint.Y}, Z={localPoint.Z}");
            Console.WriteLine($"Class (Ref Type) Value: {localScalar.Value}");
            
            // Explanation of the difference:
            Console.WriteLine("\nExplanation:");
            Console.WriteLine("1. 'localPoint' exists directly in the current stack frame.");
            Console.WriteLine("2. 'localScalar' is a reference on the stack pointing to data on the heap.");
            Console.WriteLine("3. When we accessed 'localPoint.X', we accessed memory directly.");
            Console.WriteLine("4. No Boxing occurred during the creation of 'localPoint'.");
        }
    }
}

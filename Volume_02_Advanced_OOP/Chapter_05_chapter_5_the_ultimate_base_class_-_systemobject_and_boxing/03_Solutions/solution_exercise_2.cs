
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections;

public interface IPhysicsBody
{
    void ApplyGravity();
}

public struct HeavyBody : IPhysicsBody
{
    public double Mass;
    public void ApplyGravity() { Console.WriteLine("HeavyBody falling fast."); }
}

public struct LightBody : IPhysicsBody
{
    public double Mass;
    public void ApplyGravity() { Console.WriteLine("LightBody floating."); }
}

public class PhysicsWorld
{
    // Stores everything as object (boxing for structs)
    private ArrayList _bodies = new ArrayList();

    public void AddBody(object body)
    {
        _bodies.Add(body);
    }

    public void Simulate()
    {
        foreach (object rawBody in _bodies)
        {
            try
            {
                // Unboxing: Copying data from heap back to stack.
                // This is expensive and risky.
                HeavyBody body = (HeavyBody)rawBody; 
                body.ApplyGravity();
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Error: Unboxing failed. The object is not a HeavyBody.");
            }
        }
    }
}

public class Program
{
    public static void Main()
    {
        PhysicsWorld world = new PhysicsWorld();
        
        // Implicit boxing happens here
        world.AddBody(new HeavyBody()); 
        world.AddBody(new LightBody()); 

        world.Simulate();
    }
}

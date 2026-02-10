
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

// Source File: theory_theoretical_foundations_part7.cs
// Description: Theoretical Foundations
// ==========================================

public class AgentController
{
    // Primary State (Mutually Exclusive)
    private AgentState _currentState;

    // Secondary Attributes (Composite)
    private AgentAttributes _currentAttributes;

    public AgentController()
    {
        _currentState = AgentState.Idle;
        _currentAttributes = AgentAttributes.None;
    }

    // Method to transition primary state
    public void TransitionTo(AgentState newState)
    {
        // Validation logic to prevent invalid transitions
        if (_currentState == AgentState.Error && newState != AgentState.Idle)
        {
            Console.WriteLine("Cannot transition from Error state without reset.");
            return;
        }

        _currentState = newState;
        Console.WriteLine($"State changed to: {newState}");
    }

    // Method to set a composite attribute flag
    public void SetAttribute(AgentAttributes attribute)
    {
        _currentAttributes |= attribute;
        Console.WriteLine($"Attribute set: {attribute}. Current attributes: {_currentAttributes}");
    }

    // Method to clear a composite attribute flag
    public void ClearAttribute(AgentAttributes attribute)
    {
        _currentAttributes &= ~attribute;
        Console.WriteLine($"Attribute cleared: {attribute}. Current attributes: {_currentAttributes}");
    }

    // Method to check for specific attributes using bitwise AND
    public bool HasAttribute(AgentAttributes attribute)
    {
        return (_currentAttributes & attribute) == attribute;
    }

    // Example usage in a tensor processing scenario
    public void ProcessTensorData()
    {
        if (_currentState == AgentState.Error)
        {
            Console.WriteLine("System in error state. Halting processing.");
            return;
        }

        // Set attributes to reflect current operation
        SetAttribute(AgentAttributes.Processing | AgentAttributes.Locked);

        try
        {
            // Simulate tensor processing
            TransitionTo(AgentState.Thinking);
            
            // Check if we are waiting for external data
            if (HasAttribute(AgentAttributes.Awaiting))
            {
                Console.WriteLine("Processing paused, awaiting input...");
            }
            else
            {
                // Continue processing
            }
        }
        catch (Exception ex)
        {
            TransitionTo(AgentState.Error);
            // Clear operational attributes on error
            _currentAttributes = AgentAttributes.None;
        }
        finally
        {
            // Ensure locks are released
            ClearAttribute(AgentAttributes.Locked);
            ClearAttribute(AgentAttributes.Processing);
        }
    }
}

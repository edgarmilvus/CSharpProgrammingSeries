
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
using System.Collections;

namespace SmartHomeSystem
{
    // 1. THE TOOL INTERFACE
    // Defines the contract that all tools must follow.
    // This allows the Agent to treat different devices uniformly.
    public interface ITool
    {
        string Name { get; }
        void Execute(string command);
    }

    // 2. CONCRETE TOOLS (Passive Composition)
    // These classes implement the logic for specific actions.
    // They are "dumb" and do not decide when to run.

    public class LightBulb : ITool
    {
        // Encapsulated state: the light is either on or off.
        private bool _isOn = false;

        public string Name
        {
            get { return "Light Bulb"; }
        }

        public void Execute(string command)
        {
            if (command == "Turn On")
            {
                _isOn = true;
                Console.WriteLine("The light is now ON.");
            }
            else if (command == "Turn Off")
            {
                _isOn = false;
                Console.WriteLine("The light is now OFF.");
            }
            else
            {
                Console.WriteLine("Light Bulb does not understand that command.");
            }
        }
    }

    public class Thermostat : ITool
    {
        // Encapsulated state: current temperature setting.
        private int _temperature = 72;

        public string Name
        {
            get { return "Thermostat"; }
        }

        public void Execute(string command)
        {
            if (command.StartsWith("Set Temp: "))
            {
                // Parsing the command string to extract the temperature.
                // Note: In a more advanced system, we might use structured data,
                // but here we rely on string parsing to keep it simple.
                string tempStr = command.Substring(10);
                int newTemp;
                if (int.TryParse(tempStr, out newTemp))
                {
                    _temperature = newTemp;
                    Console.WriteLine($"Thermostat set to {_temperature} degrees.");
                }
                else
                {
                    Console.WriteLine("Invalid temperature format.");
                }
            }
            else
            {
                Console.WriteLine("Thermostat does not understand that command.");
            }
        }
    }

    // 3. THE AGENT (Active Composition)
    // The Agent possesses tools and makes decisions.
    public class HomeAssistantAgent
    {
        // The Agent holds a collection of tools.
        // We use an ArrayList because Generics are forbidden in this section.
        // ArrayList stores objects of type 'object', so we must cast when retrieving.
        private ArrayList _tools;

        public HomeAssistantAgent()
        {
            _tools = new ArrayList();
        }

        // Method to dynamically compose the toolset.
        // This demonstrates dependency injection (passing dependencies in).
        public void AddTool(ITool tool)
        {
            _tools.Add(tool);
            Console.WriteLine($"Agent registered tool: {tool.Name}");
        }

        // The core logic: parsing input and delegating to the correct tool.
        public void ProcessRequest(string userRequest)
        {
            Console.WriteLine($"\n--- Processing Request: '{userRequest}' ---");

            ITool selectedTool = null;
            string command = "";

            // Simple parsing logic (Decision Making)
            if (userRequest.Contains("light"))
            {
                if (userRequest.Contains("on")) command = "Turn On";
                else if (userRequest.Contains("off")) command = "Turn Off";
                
                // Find the tool by iterating through the collection
                selectedTool = FindTool("Light Bulb");
            }
            else if (userRequest.Contains("temperature") || userRequest.Contains("thermostat"))
            {
                if (userRequest.Contains("75")) command = "Set Temp: 75";
                else if (userRequest.Contains("68")) command = "Set Temp: 68";

                selectedTool = FindTool("Thermostat");
            }

            // Execute the tool if found
            if (selectedTool != null && command != "")
            {
                Console.WriteLine($"Agent selecting tool: {selectedTool.Name}");
                selectedTool.Execute(command);
            }
            else
            {
                Console.WriteLine("Agent could not determine how to handle the request.");
            }
        }

        // Helper method to locate a tool in the ArrayList.
        // Since ArrayList stores 'object', we must cast to ITool.
        private ITool FindTool(string toolName)
        {
            // Manual iteration is required because LINQ is forbidden.
            for (int i = 0; i < _tools.Count; i++)
            {
                object item = _tools[i];
                // Explicit cast to the interface
                ITool tool = (ITool)item;
                
                if (tool.Name == toolName)
                {
                    return tool;
                }
            }
            return null;
        }
    }

    // 4. USAGE EXAMPLE
    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate the Agent
            HomeAssistantAgent agent = new HomeAssistantAgent();

            // Instantiate Tools
            ITool light = new LightBulb();
            ITool temp = new Thermostat();

            // Compose the Agent with Tools (Active Composition)
            agent.AddTool(light);
            agent.AddTool(temp);

            // Simulate User Requests
            agent.ProcessRequest("Can you turn the light on?");
            agent.ProcessRequest("Set the temperature to 75 degrees.");
            agent.ProcessRequest("Turn off the light");
            
            // Attempting a request with an unregistered tool or command
            agent.ProcessRequest("Play music"); 
        }
    }
}

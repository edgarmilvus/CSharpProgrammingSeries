
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;

namespace AdvancedChatBot
{
    // 1. Define the data structure for the Chat Context.
    // This encapsulates the user's message and any metadata we might need.
    public class ChatContext
    {
        public string UserMessage { get; set; }
        public string UserId { get; set; }

        public ChatContext(string user, string message)
        {
            UserId = user;
            UserMessage = message;
        }
    }

    // 2. Define the Delegate Type.
    // A delegate is a type that represents references to methods with a particular parameter list and return type.
    // Here, we define a delegate named 'SkillHandler' that points to any method accepting a ChatContext and returning a string.
    // This is the contract that all our plugins must adhere to.
    public delegate string SkillHandler(ChatContext context);

    // 3. The Facade / Core Chatbot Class.
    // This class manages the plugins and routes incoming messages to the correct handler.
    public class ModularChatbot
    {
        // A dictionary to map keywords (strings) to specific handlers (delegates).
        // This acts as our registry for "hot-swappable" plugins.
        private Dictionary<string, SkillHandler> _skillRegistry;

        public ModularChatbot()
        {
            _skillRegistry = new Dictionary<string, SkillHandler>();
        }

        // Method to register a new plugin (skill) dynamically.
        public void RegisterSkill(string triggerKeyword, SkillHandler handler)
        {
            // In a real system, we might validate for duplicates or conflicts here.
            _skillRegistry[triggerKeyword] = handler;
            Console.WriteLine($"[System] New Skill Registered: '{triggerKeyword}'");
        }

        // The main entry point for processing user input.
        public void ProcessMessage(ChatContext context)
        {
            Console.WriteLine($"\n[Bot] Received: '{context.UserMessage}'");

            // We simulate a simple intent recognition by looking for keywords.
            // In Book 3, we will replace this with actual Tensor processing.
            bool handled = false;
            
            // Iterate through registered skills to find a match.
            // Note: In a real production system, we would use more sophisticated matching algorithms.
            foreach (var skill in _skillRegistry)
            {
                if (context.UserMessage.ToLower().Contains(skill.Key.ToLower()))
                {
                    // Execute the delegate (invoke the plugin logic).
                    string response = skill.Value.Invoke(context);
                    Console.WriteLine($"[Bot] Response: {response}");
                    handled = true;
                    break; // Stop after the first match for this simple example.
                }
            }

            if (!handled)
            {
                Console.WriteLine("[Bot] Response: I'm sorry, I don't have a skill for that yet.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate the core chatbot.
            ModularChatbot bot = new ModularChatbot();

            // --- PLUG-IN SIMULATION ---
            // In a real application, these plugins might be loaded from DLL files.
            // Here, we define them as Lambda Expressions directly in the code for demonstration.

            // Plugin 1: Order Status
            // We use a Lambda Expression (context => { ... }) to define the logic inline.
            // This is concise and avoids the need to define separate named methods for simple tasks.
            SkillHandler orderStatusLogic = (context) => 
            {
                // Simulate looking up a database.
                return $"Order #12345 for user {context.UserId} is currently 'Shipped'.";
            };
            bot.RegisterSkill("order", orderStatusLogic);

            // Plugin 2: Returns Policy
            // Another lambda expression defining the return policy logic.
            bot.RegisterSkill("return", (context) => 
            {
                return "Returns must be initiated within 30 days of purchase.";
            });

            // Plugin 3: Technical Support
            // This lambda captures a local variable (supportEmail), demonstrating closure.
            string supportEmail = "support@example.com";
            bot.RegisterSkill("help", (context) => 
            {
                return $"Technical issue? Please email {supportEmail} or call 555-0199.";
            });

            // --- SIMULATING CONVERSATIONS ---
            // Now we test the system with various inputs.

            // Test 1: Matches 'order' keyword
            bot.ProcessMessage(new ChatContext("UserAlice", "Where is my order?"));

            // Test 2: Matches 'return' keyword
            bot.ProcessMessage(new ChatContext("UserBob", "I want to return an item."));

            // Test 3: Matches 'help' keyword
            bot.ProcessMessage(new ChatContext("UserCharlie", "I need help with my router."));

            // Test 4: No matching plugin
            bot.ProcessMessage(new ChatContext("UserDave", "What is the weather like?"));
            
            // --- HOT SWAPPING DEMONSTRATION ---
            // We can overwrite an existing skill or add a new one at runtime.
            Console.WriteLine("\n--- UPDATING PLUGINS AT RUNTIME ---");
            
            // Overwrite the 'order' skill with a more detailed version.
            bot.RegisterSkill("order", (context) => 
            {
                return $"[UPDATED] User {context.UserId}, Order #12345 is 'Out for Delivery'. Estimated arrival: Today.";
            });

            // Test the updated skill
            bot.ProcessMessage(new ChatContext("UserAlice", "Check my order status"));
        }
    }
}

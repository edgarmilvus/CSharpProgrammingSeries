
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
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JarvisCoreAssistant
{
    // Core Context: Mimicking the Semantic Kernel's Plugin architecture.
    // In a real Jarvis system, these would be decorated with [KernelFunction] attributes.
    // Here, we use explicit interface definitions to demonstrate the "Orchestrating Multi-Modal Plugins"
    // concept from Chapter 20.
    public interface IJarvisPlugin
    {
        string Name { get; }
        Task<string> ExecuteAsync(string input);
    }

    // Plugin 1: Native Windows System Control (File Management)
    // Real-world context: A user wants to quickly archive logs or organize screenshots.
    public class FileSystemPlugin : IJarvisPlugin
    {
        public string Name => "FileSystem";

        public async Task<string> ExecuteAsync(string input)
        {
            // Parsing the input to determine intent (Simple NLP logic)
            if (input.Contains("archive") && input.Contains("logs"))
            {
                try
                {
                    string sourceDir = @"C:\Temp\Logs";
                    string destDir = @"C:\Temp\Archive";

                    if (!Directory.Exists(sourceDir))
                        return $"Error: Source directory '{sourceDir}' does not exist.";

                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    var files = Directory.GetFiles(sourceDir);
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(destDir, fileName);
                        File.Move(file, destFile);
                    }

                    await Task.Delay(100); // Simulate I/O latency
                    return $"Success: Archived {files.Length} log files to '{destDir}'.";
                }
                catch (Exception ex)
                {
                    return $"Error: File operation failed. {ex.Message}";
                }
            }

            return "FileSystem Plugin: Command not recognized. Try 'archive logs'.";
        }
    }

    // Plugin 2: Persistent Memory & Personalization
    // Real-world context: The assistant remembers user preferences (e.g., preferred text editor).
    public class MemoryPlugin : IJarvisPlugin
    {
        // In a full Semantic Kernel implementation, this would be an IVolatileMemoryStore.
    // Here, we simulate persistent storage using a simple Dictionary and file I/O.
    private Dictionary<string, string> _memoryStore = new Dictionary<string, string>();
    private readonly string _memoryFilePath = "jarvis_memory.dat";

    public string Name => "Memory";

    public MemoryPlugin()
    {
        LoadMemory();
    }

    public async Task<string> ExecuteAsync(string input)
    {
        // Logic to distinguish between saving and retrieving memory
        if (input.StartsWith("remember "))
        {
            // Format: "remember [key] is [value]"
            int keyIndex = input.IndexOf("remember ") + 9;
            int separatorIndex = input.IndexOf(" is ");
            
            if (separatorIndex > keyIndex)
            {
                string key = input.Substring(keyIndex, separatorIndex - keyIndex).Trim();
                string value = input.Substring(separatorIndex + 4).Trim();
                
                _memoryStore[key] = value;
                SaveMemory();
                return $"Roger that. I will remember that {key} is {value}.";
            }
        }
        else if (input.StartsWith("recall "))
        {
            string key = input.Substring(7).Trim();
            if (_memoryStore.ContainsKey(key))
            {
                return $"Recalling: {key} is {_memoryStore[key]}.";
            }
            return "I don't have that information yet.";
        }

        return "Memory Plugin: Use 'remember [key] is [value]' or 'recall [key]'.";
    }

    private void SaveMemory()
    {
        var sb = new StringBuilder();
        foreach (var kvp in _memoryStore)
        {
            sb.AppendLine($"{kvp.Key}|{kvp.Value}");
        }
        File.WriteAllText(_memoryFilePath, sb.ToString());
    }

    private void LoadMemory()
    {
        if (File.Exists(_memoryFilePath))
        {
            var lines = File.ReadAllLines(_memoryFilePath);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 2)
                {
                    _memoryStore[parts[0]] = parts[1];
                }
            }
        }
    }
}

    // Plugin 3: Proactive Notification System
    // Real-world context: Alerting the user when a long-running task is complete.
    public class NotificationPlugin : IJarvisPlugin
    {
        public string Name => "Notification";

        public async Task<string> ExecuteAsync(string input)
        {
            // Simulating a Windows Toast Notification or Console Alert
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[NOTIFICATION]: {input}");
            Console.ResetColor();

            // In a real Windows Service, this would trigger a Microsoft.Toolkit.Uwp.Notifications.ToastNotification
            await Task.Delay(50); // Simulate UI thread dispatch
            return "Notification sent.";
        }
    }

    // The Orchestrator (Semantic Kernel Core)
    // This class mimics the Kernel's role in routing prompts to functions.
    public class JarvisKernel
    {
        private readonly List<IJarvisPlugin> _plugins;

        public JarvisKernel()
        {
            _plugins = new List<IJarvisPlugin>();
        }

        public void RegisterPlugin(IJarvisPlugin plugin)
        {
            _plugins.Add(plugin);
            Console.WriteLine($"[System]: Plugin '{plugin.Name}' registered.");
        }

        // The "Planner" Logic
        // This method parses the user input and delegates to the appropriate plugin.
        // It mimics the Agentic Workflow Design where the kernel decides the execution path.
        public async Task<string> InvokeAsync(string userRequest)
        {
            // Basic intent routing (simplified Planner)
            if (userRequest.Contains("archive") || userRequest.Contains("logs"))
            {
                var fsPlugin = _plugins.Find(p => p.Name == "FileSystem");
                if (fsPlugin != null) return await fsPlugin.ExecuteAsync(userRequest);
            }
            
            if (userRequest.Contains("remember") || userRequest.Contains("recall"))
            {
                var memPlugin = _plugins.Find(p => p.Name == "Memory");
                if (memPlugin != null) return await memPlugin.ExecuteAsync(userRequest);
            }

            // Fallback logic for generic commands
            if (userRequest.Contains("notify"))
            {
                var notifPlugin = _plugins.Find(p => p.Name == "Notification");
                if (notifPlugin != null) 
                {
                    // Extract the message to notify
                    string message = userRequest.Replace("notify", "").Trim();
                    return await notifPlugin.ExecuteAsync(message);
                }
            }

            return "Command not understood. Available commands: archive logs, remember [x], recall [x], notify [message].";
        }
    }

    // Main Application Entry Point
    // Represents the Background Service Integration.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Jarvis Core Assistant (Background Service Mode)";
            Console.WriteLine("Initializing Jarvis Kernel v1.0...");
            Console.WriteLine("Loading Native Windows Plugins...\n");

            // 1. Initialize Kernel
            var kernel = new JarvisKernel();

            // 2. Register Multi-Modal Plugins
            kernel.RegisterPlugin(new FileSystemPlugin());
            kernel.RegisterPlugin(new MemoryPlugin());
            kernel.RegisterPlugin(new NotificationPlugin());

            // 3. Main Interaction Loop (Simulating a persistent background service)
            bool isRunning = true;
            while (isRunning)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nUser > ");
                Console.ResetColor();
                
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.ToLower() == "exit") break;

                // 4. Kernel Orchestration
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Jarvis > ");
                Console.ResetColor();

                // Simulate async processing typical of LLM calls
                string response = await kernel.InvokeAsync(input);
                Console.WriteLine(response);
            }

            Console.WriteLine("\nJarvis Service Stopped.");
        }
    }
}

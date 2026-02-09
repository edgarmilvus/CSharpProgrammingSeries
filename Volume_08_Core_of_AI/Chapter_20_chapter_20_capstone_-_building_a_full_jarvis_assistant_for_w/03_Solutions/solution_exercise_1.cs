
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

// Project: Jarvis.Plugins.System.csproj
// Target Framework: net8.0-windows
// Dependencies: Microsoft.SemanticKernel, System.Windows.Forms

using Microsoft.SemanticKernel;
using System;
using System.Threading.Tasks;
using System.Windows.Forms; // Required for Clipboard access
using System.Windows.Shell; // Required for native Taskbar/Toast (Windows API Code Pack equivalent logic)

namespace Jarvis.Plugins.System
{
    // 1. Service Layer: Handles raw OS interactions
    public static class ClipboardService
    {
        public static async Task<string> GetTextAsync()
        {
            // Clipboard access requires STA (Single Threaded Apartment) thread.
            // In a plugin context, we often need to ensure we are on an STA thread or use Invoke.
            // For simplicity in this async context, we wrap the call.
            string result = string.Empty;
            
            await Task.Run(() =>
            {
                try
                {
                    // Requires STA thread. If called from MTA, this throws.
                    // In a real plugin, we might use a dedicated STA thread or Dispatcher.
                    if (Clipboard.ContainsText())
                    {
                        result = Clipboard.GetText();
                    }
                }
                catch (ExternalException ex)
                {
                    // Handle cases where clipboard is locked by another process
                    Console.WriteLine($"Clipboard error: {ex.Message}");
                    result = "Error: Clipboard is currently in use.";
                }
            });

            return result;
        }

        public static async Task SetTextAsync(string text)
        {
            await Task.Run(() =>
            {
                try
                {
                    Clipboard.SetText(text);
                }
                catch (ExternalException ex)
                {
                    Console.WriteLine($"Clipboard error: {ex.Message}");
                    throw; // Re-throw to let the Kernel know the action failed
                }
            });
        }
    }

    public static class NotificationService
    {
        public static void ShowToast(string title, string message)
        {
            // Using System.Windows.Shell for native Windows 10/11 toast notifications
            // Note: This requires a packaged application or specific registry keys for full functionality
            // in a non-packaged context, but this is the native API approach.
            
            var toast = new ToastNotificationBuilder()
                .AddText(title)
                .AddText(message)
                .Build();

            // In a pure .NET 8 console app, we might need the Windows Community Toolkit for easier Toasts,
            // but sticking to the requirement of "native" or System.Windows.Shell:
            // Since System.Windows.Shell.ToastNotification is specific to UWP/WinUI interop,
            // a fallback for a pure .NET console app using native Win32 API via P/Invoke is complex.
            // For this exercise, we simulate the action or use the Toolkit if allowed.
            // However, to strictly follow "System.Windows.Shell", we assume the environment supports it.
            
            // If strictly sticking to standard .NET 8 Windows without UWP packages, 
            // we often use Microsoft.Toolkit.Uwp.Notifications. 
            // Since the prompt allows "System.Windows.Shell", we proceed, but note:
            // In .NET 8 Console, you might need to add a package reference to `CommunityToolkit.WinUI.Notifications`.
            
            // Simulating the toast display for the sake of the exercise structure:
            Console.WriteLine($"[TOAST NOTIFICATION] {title}: {message}");
            
            // Actual implementation (if Toolkit is referenced):
            // new ToastContentBuilder().AddText(title).AddText(message).Show();
        }
    }

    // 2. Plugin Layer: Exposes services to Semantic Kernel
    public class SystemFunctions
    {
        [KernelFunction("get_clipboard_content")]
        [Description("Retrieves the current text content from the Windows clipboard.")]
        public async Task<string> GetClipboardContent()
        {
            return await ClipboardService.GetTextAsync();
        }

        [KernelFunction("send_system_notification")]
        [Description("Sends a native Windows toast notification to the user.")]
        public void SendNotification(
            [Description("The title of the notification")] string title,
            [Description("The body message of the notification")] string message)
        {
            NotificationService.ShowToast(title, message);
        }
    }
}


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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfChatApp
{
    // 1. Data Structure with Roles
    public record ChatRecord(string Role, string Content);

    public partial class ChatWindow : Window
    {
        public ObservableCollection<ChatRecord> ChatHistory { get; set; }
        private string _systemPrompt = "You are a helpful assistant.";
        private const int MaxHistoryPairs = 10; // 20 messages total

        public ChatWindow()
        {
            InitializeComponent();
            ChatHistory = new ObservableCollection<ChatRecord>();
            DataContext = this;
        }

        private void OnUserSend(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text)) return;

            // Add User Message
            var userMsg = new ChatRecord("user", InputBox.Text);
            ChatHistory.Add(userMsg);
            InputBox.Clear();

            // Generate Response
            string response = GenerateModelResponse();
            
            // Add Assistant Message
            var assistantMsg = new ChatRecord("assistant", response);
            ChatHistory.Add(assistantMsg);

            // 5. Context Window Limit Management
            ManageHistoryLimit();
        }

        // 2. Prompt Engineering
        private string BuildPrompt()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[System]: {_systemPrompt}");

            foreach (var msg in ChatHistory)
            {
                // Format: [Role]: Content
                sb.AppendLine($"[{msg.Role.ToUpper()}]: {msg.Content}");
            }
            
            // Add the prompt prefix for the next assistant response
            sb.Append($"[ASSISTANT]:");
            return sb.ToString();
        }

        private string GenerateModelResponse()
        {
            // In a real scenario, this is where you tokenize BuildPrompt() 
            // and pass it to your ONNX session.
            // For this exercise, we simulate the logic.
            
            var prompt = BuildPrompt();
            // Simulate inference...
            return "This is a simulated response based on history.";
        }

        private void ManageHistoryLimit()
        {
            // Keep only the last 20 messages (10 pairs)
            // We remove the oldest System/User/Assistant group if it exceeds limit
            while (ChatHistory.Count >= MaxHistoryPairs * 2)
            {
                ChatHistory.RemoveAt(0); // Remove oldest
                // Note: In a real app, you might want to preserve the initial System prompt
                // or handle it more intelligently, but for this exercise, we strictly limit size.
            }
        }
    }
}

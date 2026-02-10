
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LlamaSharpExercises
{
    public enum Role { System, User, Assistant }

    public record ChatMessage(Role Role, string Content);

    public class ConversationHistory
    {
        private List<ChatMessage> _messages = new List<ChatMessage>();
        private readonly int _contextSize;
        private readonly LlamaContext _context; // Reference for token counting

        public ConversationHistory(LlamaContext context, int contextSize)
        {
            _context = context;
            _contextSize = contextSize;
        }

        public void AddMessage(ChatMessage message)
        {
            _messages.Add(message);
            ManageContextWindow();
        }

        public void SetSystemMessage(string content)
        {
            var existingSystem = _messages.FirstOrDefault(m => m.Role == Role.System);
            if (existingSystem != null)
            {
                // Replace existing
                var index = _messages.IndexOf(existingSystem);
                _messages[index] = new ChatMessage(Role.System, content);
            }
            else
            {
                // Insert at beginning
                _messages.Insert(0, new ChatMessage(Role.System, content));
            }
            
            // Re-check window because system message token count might change total
            ManageContextWindow();
        }

        public string BuildPrompt()
        {
            // Llama 3 Template Example
            // <|begin_of_text|><|start_header_id|>system<|end_header_id|>
            // You are a helpful assistant.<|eot_id|><|start_header_id|>user<|end_header_id|>
            // Hello!<|eot_id|><|start_header_id|>assistant<|end_header_id|>
            
            var sb = new StringBuilder();
            sb.Append("<|begin_of_text|>");

            foreach (var msg in _messages)
            {
                sb.Append($"<|start_header_id|>{msg.Role.ToString().ToLower()}<|end_header_id|>\n");
                sb.Append($"{msg.Content}\n");
                sb.Append($"<|eot_id|>\n");
            }

            // Add the assistant header to prompt the model to start generating
            sb.Append($"<|start_header_id|>assistant<|end_header_id|>\n");

            return sb.ToString();
        }

        public int CountTokens(string prompt)
        {
            // Use the model's tokenizer to get accurate count
            // false = don't add BOS/EOS, as the template handles special tokens
            return _context.Model.Tokenize(prompt, false).Length;
        }

        private void ManageContextWindow()
        {
            // Check if we are approaching the limit (e.g., 80%)
            // We need to rebuild the prompt to count tokens accurately
            string currentPrompt = BuildPrompt();
            int currentTokens = CountTokens(currentPrompt);

            if (currentTokens > _contextSize * 0.8)
            {
                Console.WriteLine("[System] Context window approaching limit. Truncating history...");
                
                // Sliding Window: Remove oldest User/Assistant pairs, keep System
                // We iterate backwards to safely remove items
                for (int i = _messages.Count - 1; i >= 0; i--)
                {
                    if (_messages[i].Role == Role.System) continue;

                    // Remove the pair (User + Assistant)
                    // This is a simplified logic; usually, we remove the oldest user/assistant exchange.
                    _messages.RemoveAt(i);
                    
                    // Recheck count
                    currentPrompt = BuildPrompt();
                    currentTokens = CountTokens(currentPrompt);
                    
                    if (currentTokens <= _contextSize * 0.8) break;
                }
            }
        }

        public void Clear() => _messages.Clear();
        public List<ChatMessage> GetHistory() => _messages;
    }
}

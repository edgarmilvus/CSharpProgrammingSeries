
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;

public class VirtualizedChatController
{
    private List<string> _messages = new();
    
    // State tracking
    private double _scrollHeight = 0;
    private double _clientHeight = 0;
    private double _lastKnownScrollTop = 0;
    
    // Threshold to consider user "at bottom" (e.g., within 10px)
    private const double BottomThreshold = 10.0;

    // Simulated UI methods (In a real app, these would interface with Blazor/React/JSInterop)
    private void UpdateUI() { /* Triggers re-render of the list */ }
    private void SetScrollPosition(double position) { /* Sets DOM scrollTop */ }
    private double GetScrollHeight() { return _scrollHeight; } // Would query DOM
    private double GetClientHeight() { return _clientHeight; } // Would query DOM
    private double GetScrollTop() { return _lastKnownScrollTop; } // Would query DOM

    // TODO: Implement the update logic
    public void OnStreamUpdate(string newToken)
    {
        // 1. Update data model
        // Assuming we append to the last message or create a new one.
        // For simplicity, let's append to the last string in the list.
        if (_messages.Count == 0)
        {
            _messages.Add(newToken);
        }
        else
        {
            _messages[^1] += newToken; // Append to the last message
        }

        // 2. Check scroll state BEFORE updating UI
        // We need to determine if the user is currently at the bottom.
        // In a real DOM, we calculate: (ScrollHeight - ScrollTop - ClientHeight) < Threshold
        double currentScrollTop = GetScrollTop();
        double currentScrollHeight = GetScrollHeight();
        double currentClientHeight = GetClientHeight();
        
        bool isAtBottom = (currentScrollHeight - currentScrollTop - currentClientHeight) < BottomThreshold;

        // 3. Calculate new height (Estimation)
        // Since we don't have the actual DOM measurement yet, we estimate.
        // Assume average char width and line height.
        int charsPerLine = 80; // Approximate
        double lineHeight = 20; // Approximate pixel height
        int lines = (int)Math.Ceiling((double)newToken.Length / charsPerLine);
        double estimatedDelta = lines * lineHeight;

        // 4. Update UI
        UpdateUI();

        // 5. Adjust scroll position
        if (isAtBottom)
        {
            // If user was at bottom, keep them at the bottom (auto-scroll)
            // Note: In a real scenario, we might calculate the new exact ScrollHeight 
            // and set ScrollTop = ScrollHeight - ClientHeight.
            // Here we simulate the logic.
            double newScrollHeight = currentScrollHeight + estimatedDelta;
            SetScrollPosition(newScrollHeight); 
        }
        else
        {
            // User is reading history. Do not adjust scroll position.
            // The new content is added above/below, but the viewport stays put.
            // We might need to compensate if the new content is added *above* the viewport
            // (e.g., in a "load more" scenario), but for appending to bottom, 
            // leaving ScrollTop untouched is correct.
        }
    }
}


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

# Source File: theory_theoretical_foundations_part9.cs
# Description: Theoretical Foundations
# ==========================================

public class BotTests
{
    public static void RunTests()
    {
        Console.WriteLine("--- Running Bot Tests ---");

        // Test 1: Word Matching
        bool test1 = WordMatcher.ContainsWord("Hello world", "world");
        Console.WriteLine($"Test 'world' in 'Hello world': {(test1 ? "PASS" : "FAIL")}");

        // Test 2: Case Sensitivity (Edge Case)
        bool test2 = WordMatcher.ContainsWord("Hello World", "world");
        Console.WriteLine($"Test 'world' in 'Hello World' (Case sensitive): {(test2 ? "FAIL (Expected)" : "PASS (Expected Failure)")}");
        
        // Test 3: Engine Response
        LearningBotEngine bot = new LearningBotEngine();
        bot.Learn("test", "This is a test response.");
        string response = bot.GetResponse("This is a test");
        Console.WriteLine($"Test Bot Response: {(response == "This is a test response." ? "PASS" : "FAIL")}");

        Console.WriteLine("--- Tests Complete ---");
    }
}

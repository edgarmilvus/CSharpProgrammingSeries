
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

// Source File: theory_theoretical_foundations_part3.cs
// Description: Theoretical Foundations
// ==========================================

public void DemonstrateDeferredExecution()
{
    var numbers = new List<int> { 1, 2, 3, 4, 5 };
    
    // 1. Query defined here. NO execution occurs.
    // The variable 'query' is an iterator, not a collection.
    var query = numbers.Where(n => n % 2 == 0).Select(n => n * 2);

    // 2. Modify the source AFTER the query is defined.
    numbers.Add(6); 

    // 3. Execution occurs here (inside the loop).
    // Result includes the modified data: 4, 6, 8, 12 (2*2, 2*3, 2*4, 2*6)
    foreach (var result in query)
    {
        Console.WriteLine(result); 
    }
}

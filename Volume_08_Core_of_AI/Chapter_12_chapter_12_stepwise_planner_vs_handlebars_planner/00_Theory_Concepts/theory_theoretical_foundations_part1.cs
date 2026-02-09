
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

# Source File: theory_theoretical_foundations_part1.cs
# Description: Theoretical Foundations
# ==========================================

// Conceptual representation of how Handlebars maps to C# Delegates
// This is not the actual internal code, but illustrates the architectural pattern.

public class HandlebarsPlan
{
    private readonly string _template;
    private readonly Kernel _kernel;

    public HandlebarsPlan(string template, Kernel kernel)
    {
        _template = template;
        _kernel = kernel;
    }

    public string Execute(object context)
    {
        // 1. Compile the template (usually done once, cached for performance)
        var compiledTemplate = Handlebars.Compile(_template);

        // 2. Register helpers that bridge to Kernel Functions
        // This is where the magic happens: mapping template syntax to C# methods
        Handlebars.RegisterHelper("invoke_function", (output, context, args) => 
        {
            string functionName = args[0].ToString();
            // Resolve the function from the Kernel's registry
            var function = _kernel.Functions.GetFunction(functionName);
            
            // Invoke the function (could be native C# or prompt)
            var result = _kernel.RunAsync(function, new KernelArguments(args.Skip(1).ToArray()));
            
            output.Write(result.Result);
        });

        // 3. Render the template with the context
        return compiledTemplate(context);
    }
}

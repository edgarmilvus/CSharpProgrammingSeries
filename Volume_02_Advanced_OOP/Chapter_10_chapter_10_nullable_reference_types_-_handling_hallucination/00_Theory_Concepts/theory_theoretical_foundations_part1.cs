
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

using System;

namespace AI.DataStructures
{
    // A generic struct representing a value that may or may not exist.
    // This prevents the use of null references for optional data.
    public struct Option<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        // Private constructor to enforce creation via static methods
        private Option(T value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        // Factory method to create a Some (present) state
        public static Option<T> Some(T value)
        {
            if (value == null) 
                throw new InvalidOperationException("Cannot wrap a null value in Option<T>. Use None instead.");
            
            return new Option<T>(value, true);
        }

        // Factory method to create a None (absent) state
        public static Option<T> None()
        {
            return new Option<T>(default(T), false);
        }

        // Property to check existence
        public bool HasValue => _hasValue;

        // Unsafe accessor - throws if value is missing
        public T Value
        {
            get
            {
                if (!_hasValue) throw new InvalidOperationException("Option has no value.");
                return _value;
            }
        }

        // Safe retrieval with fallback
        public T GetValueOrDefault(T defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }
    }
}

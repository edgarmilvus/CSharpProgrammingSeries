
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;

namespace Book3_Chapter5_PracticalExercises
{
    public class Exercise1_Solution
    {
        public class CustomDictionary<TKey, TValue> where TKey : IEquatable<TKey>
        {
            private struct Entry
            {
                public int HashCode;
                public TKey Key;
                public TValue Value;
                public int Next; // Index of next entry in collision chain
            }

            private int[] _buckets;
            private Entry[] _entries;
            private int _count;
            private int _freeList;
            private int _freeCount;

            public CustomDictionary(int capacity = 10)
            {
                // Initialize with a prime number size for better distribution
                int size = PrimeHelper.GetPrime(capacity);
                _buckets = new int[size];
                for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
                
                _entries = new Entry[size];
                _freeList = -1;
            }

            public void Add(TKey key, TValue value)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                // 1. Calculate Hash
                int hashCode = key.GetHashCode() & 0x7FFFFFFF; // Ensure positive
                int targetBucket = hashCode % _buckets.Length;

                // 2. Collision Resolution: Chaining
                // Traverse the linked list in the bucket to check for duplicates
                for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
                {
                    if (_entries[i].HashCode == hashCode && _entries[i].Key.Equals(key))
                        throw new Exception("Key already exists");
                }

                // 3. Allocate Entry (Handling free list or resizing)
                int index;
                if (_freeCount > 0)
                {
                    index = _freeList;
                    _freeList = _entries[index].Next;
                    _freeCount--;
                }
                else
                {
                    if (_count == _entries.Length)
                    {
                        Resize();
                        // Bucket index changes after resize, recalculate
                        targetBucket = hashCode % _buckets.Length; 
                    }
                    index = _count;
                    _count++;
                }

                // 4. Insert Entry
                _entries[index].HashCode = hashCode;
                _entries[index].Key = key;
                _entries[index].Value = value;
                
                // 5. Link to Bucket Head
                _entries[index].Next = _buckets[targetBucket];
                _buckets[targetBucket] = index;
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                int hashCode = key.GetHashCode() & 0x7FFFFFFF;
                int bucket = hashCode % _buckets.Length;

                // Traverse the chain
                for (int i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
                {
                    if (_entries[i].HashCode == hashCode && _entries[i].Key.Equals(key))
                    {
                        value = _entries[i].Value;
                        return true;
                    }
                }

                value = default(TValue);
                return false;
            }

            private void Resize()
            {
                // Double the size and find next prime
                int newSize = PrimeHelper.GetPrime(_count * 2);
                int[] newBuckets = new int[newSize];
                for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
                
                Entry[] newEntries = new Entry[newSize];
                Array.Copy(_entries, 0, newEntries, 0, _count);

                // Rehash all entries to fit new bucket array
                for (int i = 0; i < _count; i++)
                {
                    if (newEntries[i].HashCode >= 0)
                    {
                        int bucket = newEntries[i].HashCode % newSize;
                        newEntries[i].Next = newBuckets[bucket];
                        newBuckets[bucket] = i;
                    }
                }

                _buckets = newBuckets;
                _entries = newEntries;
            }

            // Helper for prime numbers (standard for hash table sizing)
            private static class PrimeHelper
            {
                public static int GetPrime(int min)
                {
                    for (int i = min; i < int.MaxValue; i++)
                    {
                        if (IsPrime(i)) return i;
                    }
                    return min;
                }

                private static bool IsPrime(int candidate)
                {
                    if ((candidate & 1) == 0) return candidate == 2;
                    int limit = (int)Math.Sqrt(candidate);
                    for (int i = 3; i <= limit; i += 2)
                        if (candidate % i == 0) return false;
                    return true;
                }
            }
        }
    }
}

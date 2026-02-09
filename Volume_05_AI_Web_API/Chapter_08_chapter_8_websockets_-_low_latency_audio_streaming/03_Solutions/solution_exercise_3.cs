
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
using System.Threading;
using Microsoft.Extensions.Logging;

namespace AudioStreamingApp.Exercises
{
    public class OverwriteEventArgs : EventArgs
    {
        public int BytesOverwritten { get; init; }
    }

    public class AudioRingBuffer
    {
        private readonly byte[] _buffer;
        private readonly int _capacity;
        private int _head = 0; // Write position
        private int _tail = 0; // Read position
        private long _bytesWritten = 0;
        private long _bytesRead = 0;
        private readonly object _lock = new object();

        // Interactive Challenge: Overwrite Notification
        public event EventHandler<OverwriteEventArgs>? OnOverwrite;

        public AudioRingBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new byte[capacity];
        }

        // Metadata: Sequence numbers
        public long NewestSequence => _bytesWritten;
        public long OldestSequence => _bytesRead;

        public void Write(ReadOnlySpan<byte> data)
        {
            lock (_lock)
            {
                int dataOffset = 0;
                int remaining = data.Length;

                // Check for overwrite
                if (_bytesWritten - _bytesRead + remaining > _capacity)
                {
                    int bytesToOverwrite = (int)(_bytesWritten - _bytesRead + remaining - _capacity);
                    _bytesRead += bytesToOverwrite; // Advance tail (discard old data)
                    
                    // Trigger event
                    OnOverwrite?.Invoke(this, new OverwriteEventArgs { BytesOverwritten = bytesToOverwrite });
                }

                while (remaining > 0)
                {
                    int chunkSize = Math.Min(remaining, _capacity - _head);
                    
                    // Copy data to buffer
                    data.Slice(dataOffset, chunkSize).CopyTo(_buffer.AsSpan(_head, chunkSize));
                    
                    // Update pointers
                    _head = (_head + chunkSize) % _capacity;
                    dataOffset += chunkSize;
                    remaining -= chunkSize;
                }
                
                _bytesWritten += data.Length;
            }
        }

        public int Read(Span<byte> destination, int count)
        {
            lock (_lock)
            {
                int available = (int)(_bytesWritten - _bytesRead);
                if (available == 0) return 0;

                int toRead = Math.Min(count, available);
                int totalRead = 0;
                int destOffset = 0;

                // Calculate logical read position relative to physical buffer
                // Since we might have overwritten, _tail might need adjustment based on _bytesRead vs _bytesWritten
                // However, for simplicity in this implementation, we track physical tail
                // In a pure circular buffer, we calculate position based on _bytesRead % _capacity
                
                int currentReadPos = (int)(_bytesRead % _capacity);

                while (totalRead < toRead)
                {
                    // Calculate how many bytes until wrap-around
                    int bytesUntilWrap = _capacity - currentReadPos;
                    int chunkSize = Math.Min(toRead - totalRead, bytesUntilWrap);

                    // Check if we are trying to read past what is written (Handle partial wrap cases)
                    // In this specific implementation logic, we rely on _bytesWritten vs _bytesRead to know amount available.
                    
                    destination.Slice(destOffset, chunkSize)
                        .CopyTo(_buffer.AsSpan(currentReadPos, chunkSize));

                    currentReadPos = (currentReadPos + chunkSize) % _capacity;
                    destOffset += chunkSize;
                    totalRead += chunkSize;
                }

                _bytesRead += totalRead;
                return totalRead;
            }
        }
    }

    // Watchdog Service
    public class WatchdogService
    {
        private readonly AudioRingBuffer _buffer;
        private readonly ILogger<WatchdogService> _logger;
        private ReadMode _currentMode = ReadMode.RealTime;

        public enum ReadMode { RealTime, CatchUp }

        public WatchdogService(AudioRingBuffer buffer, ILogger<WatchdogService> logger)
        {
            _buffer = buffer;
            _logger = logger;
            
            // Subscribe to overwrite events
            _buffer.OnOverwrite += (sender, args) =>
            {
                _logger.LogWarning($"Overwrite detected! {args.BytesOverwritten} bytes lost.");
                
                // Logic: If we are overwriting, the writer is faster than the reader.
                if (_currentMode == ReadMode.RealTime)
                {
                    _logger.LogInformation("Switching to CatchUp mode to clear backlog.");
                    _currentMode = ReadMode.CatchUp;
                }
            };
        }

        public void MonitorReadSpeed()
        {
            // In a real app, this would run periodically to check lags
            // If lag is detected, adjust _currentMode
        }
    }
}

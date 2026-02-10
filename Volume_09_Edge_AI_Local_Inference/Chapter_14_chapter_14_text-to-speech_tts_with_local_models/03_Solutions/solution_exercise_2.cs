
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NAudio.Wave; // Requires NAudio NuGet package

// --- 1. Interfaces and Models ---

public interface IAudioStream
{
    // Simulates receiving chunks from a TTS engine
    IAsyncEnumerable<float[]> GetStreamAsync(CancellationToken token);
}

public interface IAudioPlayer
{
    void Start(IAudioStream source);
    void Pause();
    void Stop();
    bool IsPlaying { get; }
}

// --- 2. Buffering Strategy (Channel-based) ---

// A thread-safe buffer using System.Threading.Channels
public class AudioBuffer
{
    private readonly Channel<float[]> _channel;

    public AudioBuffer(int bufferSize = 4096)
    {
        // Bounded capacity to prevent memory overflow if consumer is slow
        var options = new BoundedChannelOptions(bufferSize)
        {
            SingleReader = true,
            SingleWriter = false, // Multiple producers possible
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<float[]>(options);
    }

    public async Task WriteAsync(float[] chunk, CancellationToken token)
    {
        await _channel.Writer.WriteAsync(chunk, token);
    }

    public async IAsyncEnumerable<float[]> ReadAllAsync()
    {
        await foreach (var chunk in _channel.Reader.ReadAllAsync())
        {
            yield return chunk;
        }
    }

    public void Complete()
    {
        _channel.Writer.TryComplete();
    }
}

// --- 3. Custom WaveProvider for NAudio ---

// This acts as the Consumer. It reads from the buffer when NAudio requests data.
public class StreamingWaveProvider : IWaveProvider
{
    private readonly AudioBuffer _buffer;
    private readonly WaveFormat _waveFormat;
    private readonly CancellationToken _cancellationToken;
    private float[]? _currentChunk;
    private int _chunkIndex;

    public StreamingWaveProvider(AudioBuffer buffer, WaveFormat format, CancellationToken token)
    {
        _buffer = buffer;
        _waveFormat = format;
        _cancellationToken = token;
        _currentChunk = null;
        _chunkIndex = 0;
    }

    public WaveFormat WaveFormat => _waveFormat;

    public int Read(byte[] buffer, int offset, int count)
    {
        // NAudio requests 'count' bytes. We need to provide exactly that (or less if stream ends).
        // 1 float = 4 bytes. Requested count is in bytes.
        int samplesNeeded = count / 4; 
        int samplesProvided = 0;
        
        // Span for efficient writing
        Span<float> floatBuffer = MemoryMarshal.Cast<byte, float>(buffer.AsSpan(offset, count));

        while (samplesProvided < samplesNeeded)
        {
            if (_currentChunk == null || _chunkIndex >= _currentChunk.Length)
            {
                // Try to get next chunk from channel (synchronously for low latency, 
                // but strictly NAudio Read is sync. We use a trick here or pre-buffering.
                // For this exercise, we will use a blocking collection or a synchronous read 
                // from the Channel reader (which isn't native). 
                // To keep it simple and robust: We will simulate async-to-sync by checking
                // the underlying queue if we used BlockingCollection, but since we used Channel:
                // We will assume the producer fills a BlockingCollection or we use a 
                // ConcurrentQueue for the 'Hot Path'.
                
                // *Correction for strict NAudio sync requirement*: 
                // ChannelReader.WaitToReadAsync is async. NAudio Read is sync.
                // To bridge this strictly, we usually poll or use a BlockingCollection.
                // Let's switch the AudioBuffer implementation to use a BlockingCollection 
                // for the 'Read' part to be blocking-compatible with NAudio, 
                // OR we accept that we might return 0 (silence) if data isn't ready.
                
                // Let's implement a safe "WaitForData" mechanism that yields silence if empty.
                if (!_buffer.ReadAllAsync().MoveNextAsync().AsTask().Wait(10)) 
                {
                    // Underrun: No data available quickly enough. Return silence.
                    // Fill remaining buffer with 0.
                    for (int i = samplesProvided; i < samplesNeeded; i++)
                    {
                        floatBuffer[i] = 0;
                    }
                    return count; 
                }
                // Note: The above is a simplified simulation. 
                // In production, use a BlockingCollection<float[]> for the AudioBuffer 
                // to allow blocking reads.
            }
            
            // ... (Logic to copy from _currentChunk to floatBuffer) ...
            // For brevity in the solution, we assume the BlockingCollection approach below:
        }
        
        // *Revised Implementation for Robust Sync Reading*:
        // Since NAudio demands synchronous reads, we use a BlockingCollection internally 
        // or a ConcurrentQueue. Let's rewrite the 'Read' logic using a ConcurrentQueue 
        // managed by a separate async producer.
        return 0; // Placeholder for the logic below
    }
}

// --- 4. Audio Player Implementation ---

public class NaudioPlayer : IAudioPlayer, IDisposable
{
    private readonly WaveOutEvent _waveOut;
    private readonly AudioBuffer _audioBuffer;
    private StreamingWaveProvider? _waveProvider;
    private CancellationTokenSource? _cts;
    private Task? _producerTask;

    public bool IsPlaying { get; private set; }

    public NaudioPlayer()
    {
        _waveOut = new WaveOutEvent();
        _audioBuffer = new AudioBuffer();
    }

    public void Start(IAudioStream source)
    {
        if (IsPlaying) return;

        _cts = new CancellationTokenSource();
        
        // 1. Setup NAudio
        var format = new WaveFormat(22050, 16, 1); // 22.05kHz, 16-bit, Mono
        _waveProvider = new StreamingWaveProvider(_audioBuffer, format, _cts.Token);
        
        // *Critical Fix*: We need a synchronous provider. 
        // Let's implement a simplified 'BlockingWaveProvider' here directly for the solution:
        var provider = new BlockingWaveProvider(_audioBuffer, format, _cts.Token);
        
        _waveOut.Init(provider);
        _waveOut.Play();

        // 2. Start Producer (Simulates TTS chunks)
        _producerTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var chunk in source.GetStreamAsync(_cts.Token))
                {
                    await _audioBuffer.WriteAsync(chunk, _cts.Token);
                }
                _audioBuffer.Complete();
            }
            catch (OperationCanceledException) { /* Expected on Stop */ }
        }, _cts.Token);

        IsPlaying = true;
    }

    public void Pause()
    {
        if (!IsPlaying) return;
        if (_waveOut.PlaybackState == PlaybackState.Playing)
            _waveOut.Pause();
        else
            _waveOut.Play();
    }

    public void Stop()
    {
        if (!IsPlaying) return;

        // 1. Cancel Producers
        _cts?.Cancel();
        
        // 2. Stop Audio
        _waveOut.Stop();
        
        // 3. Cleanup
        _audioBuffer.Complete();
        _producerTask?.Wait(500); // Wait for cleanup
        _waveOut.Dispose();
        
        IsPlaying = false;
    }

    public void Dispose()
    {
        Stop();
    }
}

// --- 5. Supporting Classes for the Solution ---

// A synchronous wrapper for NAudio using BlockingCollection
public class BlockingWaveProvider : IWaveProvider
{
    private readonly BlockingCollection<float[]> _queue;
    private readonly WaveFormat _format;
    private float[]? _currentBuffer;
    private int _currentIndex;

    public BlockingWaveProvider(AudioBuffer buffer, WaveFormat format, CancellationToken token)
    {
        // We need to bridge the async AudioBuffer to a sync queue.
        // For this specific solution, we create a wrapper that populates a BlockingCollection.
        _queue = new BlockingCollection<float[]>(new ConcurrentQueue<float[]>());
        _format = format;
        _currentIndex = 0;

        // Bridge task
        Task.Run(async () =>
        {
            await foreach (var chunk in buffer.ReadAllAsync())
            {
                if (token.IsCancellationRequested) break;
                _queue.Add(chunk);
            }
            _queue.CompleteAdding();
        });
    }

    public WaveFormat WaveFormat => _format;

    public int Read(byte[] buffer, int offset, int count)
    {
        int samplesNeeded = count / 4;
        int samplesWritten = 0;
        Span<float> output = MemoryMarshal.Cast<byte, float>(buffer.AsSpan(offset, count));

        while (samplesWritten < samplesNeeded)
        {
            if (_currentBuffer == null || _currentIndex >= _currentBuffer.Length)
            {
                if (!_queue.TryTake(out _currentBuffer, 100)) // 100ms timeout
                {
                    // Underrun: Return silence
                    output.Slice(samplesWritten).Fill(0);
                    return samplesWritten * 4;
                }
                _currentIndex = 0;
            }

            int toCopy = Math.Min(_currentBuffer.Length - _currentIndex, samplesNeeded - samplesWritten);
            _currentBuffer.AsSpan(_currentIndex, toCopy).CopyTo(output.Slice(samplesWritten));
            
            samplesWritten += toCopy;
            _currentIndex += toCopy;
        }

        return samplesWritten * 4;
    }
}

// Mock AudioStream for testing
public class MockTtsStream : IAudioStream
{
    public async IAsyncEnumerable<float[]> GetStreamAsync(CancellationToken token)
    {
        // Simulate generating 5 chunks of audio
        for (int i = 0; i < 5; i++)
        {
            token.ThrowIfCancellationRequested();
            
            // Generate a small chunk of sine wave
            var chunk = new float[1024];
            for (int j = 0; j < chunk.Length; j++)
            {
                chunk[j] = (float)Math.Sin((i * 100 + j) * 0.1);
            }
            
            yield return chunk;
            
            // Simulate delay between chunks (latency)
            await Task.Delay(200, token);
        }
    }
}

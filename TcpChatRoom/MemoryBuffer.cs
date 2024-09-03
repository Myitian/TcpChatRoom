using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpChatRoom
{
    public class MemoryBuffer<T> : IDisposable
    {
        public static readonly MemoryBuffer<T> Empty = new(Memory<T>.Empty);

        private bool disposed = false;
        private T[]? underlyingBuffer;
        private readonly Source source;

        public int Length => Memory.Length;
        public Memory<T> Memory { get; }
        public Span<T> Span => Memory.Span;

        public MemoryBuffer(int length)
        {
            source = Source.ArrayPool;
            underlyingBuffer = ArrayPool<T>.Shared.Rent(length);
            Memory = underlyingBuffer.AsMemory(0, length);
        }
        public MemoryBuffer(T[]? buffer)
        {
            source = Source.Array;
            underlyingBuffer = buffer;
            Memory = underlyingBuffer;
        }
        public MemoryBuffer(Memory<T> buffer)
        {
            source = Source.Memory;
            underlyingBuffer = null;
            Memory = buffer;
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (source is Source.ArrayPool && underlyingBuffer is not null)
                        ArrayPool<T>.Shared.Return(underlyingBuffer);
                }
                underlyingBuffer = null;
                disposed = true;
            }
        }

        ~MemoryBuffer()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private enum Source
        {
            ArrayPool,
            Array,
            Memory
        }

        public static implicit operator Span<T>(MemoryBuffer<T> buffer)
            => buffer.Span;

        public static implicit operator Memory<T>(MemoryBuffer<T> buffer)
            => buffer.Memory;

        public static explicit operator MemoryBuffer<T>(Memory<T> buffer)
            => new(buffer);

        public static explicit operator MemoryBuffer<T>(T[] buffer)
            => new(buffer);
    }
}

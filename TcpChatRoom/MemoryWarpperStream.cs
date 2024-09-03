using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TcpChatRoom
{
    public class MemoryWarpperStream : Stream
    {
        private Memory<byte> mem;
        private int position = 0;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite { get; }
        public override long Length => mem.Length;
        public override long Position
        {
            get => position;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Length);
                position = (int)value;
            }
        }

        public MemoryWarpperStream(ReadOnlyMemory<byte> memory)
        {
            CanWrite = false;
            mem = Unsafe.As<ReadOnlyMemory<byte>, Memory<byte>>(ref memory);
        }
        public MemoryWarpperStream(Memory<byte> memory)
        {
            CanWrite = true;
            mem = memory;
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            destination.Write(mem.Span);
        }
        public override void Flush()
        {
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }
        public override int Read(Span<byte> buffer)
        {
            if (position >= Length)
                return 0;
            ReadOnlySpan<byte> span = mem.Span[position..];
            if (buffer.Length < span.Length)
                span = span[..buffer.Length];
            span.CopyTo(buffer);
            return span.Length;
        }
        public override int ReadByte()
        {
            if (position >= Length)
                return -1;
            return mem.Span[position++];
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            long loc = origin switch
            {
                SeekOrigin.Begin => 0,
                SeekOrigin.Current => Position,
                SeekOrigin.End => Length,
                _ => throw new ArgumentException("", nameof(origin)),
            };
            long min = -loc, max = Length - loc;
            ArgumentOutOfRangeException.ThrowIfLessThan(offset, min);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, max);
            return Position = loc + offset;
        }

        public override void SetLength(long value)
        {
            if (value != Length)
                throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer.AsSpan(offset, count));
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!CanWrite)
                throw new NotSupportedException();
            long v = position + buffer.Length;
            if (v < 0 || v >= Length)
                throw new NotSupportedException();
            buffer.CopyTo(mem.Span.Slice(position, buffer.Length));
        }
        public override void WriteByte(byte value)
        {
            if (!CanWrite)
                throw new NotSupportedException();
            if (position >= Length)
                throw new NotSupportedException();
            mem.Span[position++] = value;
        }
    }
}

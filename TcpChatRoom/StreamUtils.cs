using System.IO;
using System.Text;

namespace TcpChatRoom;

public static class StreamUtils
{
    public const int PAYLOAD_BUFFER_SIZE = 4096;
    public const int VARINT_SEGMENT_BITS = 0x7F;
    public const int VARINT_CONTINUE_BIT = 0x80;

    public static void Skip(this Stream stream, long offset, bool throwOnEndOfStream = true)
    {
        Span<byte> buffer = stackalloc byte[(int)Math.Min(offset, PAYLOAD_BUFFER_SIZE)];
        while (offset > 0)
        {
            offset -= stream.ReadAtLeast(buffer, (int)Math.Min(offset, PAYLOAD_BUFFER_SIZE), throwOnEndOfStream);
        }
    }
    public static void FillPadding(this Stream stream, long offset)
    {
        ReadOnlySpan<byte> buffer = stackalloc byte[(int)Math.Min(offset, PAYLOAD_BUFFER_SIZE)];
        while (offset > 0)
        {
            int write = (int)Math.Min(offset, PAYLOAD_BUFFER_SIZE);
            stream.Write(buffer[..write]);
            offset -= write;
        }
    }

    #region Write
    public static void Write(Stream stream, bool value)
    {
        if (value)
            stream.WriteByte(1);
        else
            stream.WriteByte(0);
    }
    public static void Write(Stream stream, bool? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, byte value)
    {
        stream.WriteByte(value);
    }
    public static void Write(Stream stream, byte? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, sbyte value)
    {
        Write(stream, (byte)value);
    }
    public static void Write(Stream stream, sbyte? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, ushort value)
    {
        ReadOnlySpan<byte> buffer = [
            (byte)(value >> 8),
            (byte)value
            ];
        stream.Write(buffer);
    }
    public static void Write(Stream stream, ushort? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, short value)
    {
        Write(stream, (ushort)value);
    }
    public static void Write(Stream stream, short? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, uint value)
    {
        ReadOnlySpan<byte> buffer = [
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
            ];
        stream.Write(buffer);
    }
    public static void Write(Stream stream, uint? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, int value)
    {
        Write(stream, (uint)value);
    }
    public static void Write(Stream stream, int? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, ulong value)
    {
        ReadOnlySpan<byte> buffer = [
            (byte)(value >> 56),
            (byte)(value >> 48),
            (byte)(value >> 40),
            (byte)(value >> 32),
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
            ];
        stream.Write(buffer);
    }
    public static void Write(Stream stream, ulong? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, long value)
    {
        Write(stream, (ulong)value);
    }
    public static void Write(Stream stream, long? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, UInt128 value)
    {
        Write(stream, (ulong)(value >> 64));
        Write(stream, (ulong)value);
    }
    public static void Write(Stream stream, UInt128? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, Int128 value)
    {
        Write(stream, (UInt128)value);
    }
    public static void Write(Stream stream, Int128? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, Half value)
    {
        Write(stream, BitConverter.HalfToUInt16Bits(value));
    }
    public static void Write(Stream stream, Half? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, float value)
    {
        Write(stream, BitConverter.SingleToUInt32Bits(value));
    }
    public static void Write(Stream stream, float? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, double value)
    {
        Write(stream, BitConverter.DoubleToUInt64Bits(value));
    }
    public static void Write(Stream stream, double? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, decimal value)
    {
        Span<int> bits = stackalloc int[4];
        decimal.GetBits(value, bits);
        Write(stream, bits[0]);
        Write(stream, bits[1]);
        Write(stream, bits[2]);
        Write(stream, bits[3]);
    }
    public static void Write(Stream stream, decimal? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, TimeSpan value)
    {
        Write(stream, value.Ticks);
    }
    public static void Write(Stream stream, TimeSpan? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, DateTime value)
    {
        Span<byte> buffer = stackalloc byte[40];
        value.TryFormat(buffer, out int length, "O");
        Write(stream, buffer[..length]);
    }
    public static void Write(Stream stream, DateTime? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, DateTimeOffset value)
    {
        Span<byte> buffer = stackalloc byte[40];
        value.TryFormat(buffer, out int length, "O");
        Write(stream, buffer[..length]);
    }
    public static void Write(Stream stream, DateTimeOffset? value)
    {
        Write(stream, value.HasValue);
        if (value.HasValue)
            Write(stream, value.Value);
    }
    public static void Write(Stream stream, ReadOnlySpan<byte> value)
    {
        Write(stream, value.Length);
        stream.Write(value);
    }
    public static void Write(Stream stream, MemoryBuffer<byte> value)
    {
        Write(stream, value.Span);
    }
    public static void Write(Stream stream, ReadOnlySpan<char> value)
    {
        Encoder encoder = Encoding.UTF8.GetEncoder();
        int byteCount = encoder.GetByteCount(value, true);
        Write(stream, byteCount);
        bool completed = false;
        Span<byte> byteBuffer = stackalloc byte[Math.Min(byteCount, PAYLOAD_BUFFER_SIZE)];
        while (!completed)
        {
            encoder.Convert(value, byteBuffer, true, out int charsUsed, out int bytesUsed, out completed);
            if (charsUsed == 0 && bytesUsed == 0)
                break;
            stream.Write(byteBuffer[..bytesUsed]);
            value = value[charsUsed..];
        }
    }
    public static void Write(Stream stream, string? value)
    {
        bool existing = value is not null;
        Write(stream, existing);
        if (existing)
            Write(stream, value.AsSpan());
    }
    public static void Write(Stream stream, Guid value)
    {
        Span<byte> buffer = stackalloc byte[16];
        value.TryWriteBytes(buffer, true, out _);
        stream.Write(buffer);
    }
    #endregion Write

    #region Length
    public static int LengthOf(bool value)
    {
        return 1;
    }
    public static int LengthOf(bool? value)
    {
        return value.HasValue ? 2 : 1;
    }
    public static int LengthOf(byte value)
    {
        return 1;
    }
    public static int LengthOf(byte? value)
    {
        return value.HasValue ? 2 : 1;
    }
    public static int LengthOf(sbyte value)
    {
        return 1;
    }
    public static int LengthOf(sbyte? value)
    {
        return value.HasValue ? 2 : 1;
    }
    public static int LengthOf(ushort value)
    {
        return 2;
    }
    public static int LengthOf(ushort? value)
    {
        return value.HasValue ? 3 : 1;
    }
    public static int LengthOf(short value)
    {
        return 2;
    }
    public static int LengthOf(short? value)
    {
        return value.HasValue ? 3 : 1;
    }
    public static int LengthOf(uint value)
    {
        return 4;
    }
    public static int LengthOf(uint? value)
    {
        return value.HasValue ? 5 : 1;
    }
    public static int LengthOf(int value)
    {
        return 4;
    }
    public static int LengthOf(int? value)
    {
        return value.HasValue ? 5 : 1;
    }
    public static int LengthOf(ulong value)
    {
        return 8;
    }
    public static int LengthOf(ulong? value)
    {
        return value.HasValue ? 9 : 1;
    }
    public static int LengthOf(long value)
    {
        return 8;
    }
    public static int LengthOf(long? value)
    {
        return value.HasValue ? 9 : 1;
    }
    public static int LengthOf(UInt128 value)
    {
        return 16;
    }
    public static int LengthOf(UInt128? value)
    {
        return value.HasValue ? 17 : 1;
    }
    public static int LengthOf(Int128 value)
    {
        return 16;
    }
    public static int LengthOf(Int128? value)
    {
        return value.HasValue ? 17 : 1;
    }
    public static int LengthOf(Half value)
    {
        return 2;
    }
    public static int LengthOf(Half? value)
    {
        return value.HasValue ? 3 : 1;
    }
    public static int LengthOf(float value)
    {
        return 4;
    }
    public static int LengthOf(float? value)
    {
        return value.HasValue ? 5 : 1;
    }
    public static int LengthOf(double value)
    {
        return 8;
    }
    public static int LengthOf(double? value)
    {
        return value.HasValue ? 9 : 1;
    }
    public static int LengthOf(decimal value)
    {
        return 16;
    }
    public static int LengthOf(decimal? value)
    {
        return value.HasValue ? 17 : 1;
    }
    public static int LengthOf(TimeSpan value)
    {
        return 8;
    }
    public static int LengthOf(TimeSpan? value)
    {
        return value.HasValue ? 9 : 1;
    }
    public static int LengthOf(DateTime value)
    {
        Span<byte> buffer = stackalloc byte[40];
        value.TryFormat(buffer, out int length, "O");
        return LengthOf(buffer[..length]);
    }
    public static int LengthOf(DateTime? value)
    {
        if (value.HasValue)
            return LengthOf(value.Value) + 1;
        else
            return 1;
    }
    public static int LengthOf(DateTimeOffset value)
    {
        Span<byte> buffer = stackalloc byte[40];
        value.TryFormat(buffer, out int length, "O");
        return LengthOf(buffer[..length]);
    }
    public static int LengthOf(DateTimeOffset? value)
    {
        if (value.HasValue)
            return LengthOf(value.Value) + 1;
        else
            return 1;
    }
    public static int LengthOf(ReadOnlySpan<byte> value)
    {
        return 4 + value.Length;
    }
    public static int LengthOf(MemoryBuffer<byte> value)
    {
        return 4 + value.Length;
    }
    public static int LengthOf(ReadOnlySpan<char> value)
    {
        Encoder encoder = Encoding.UTF8.GetEncoder();
        int byteCount = encoder.GetByteCount(value, true);
        return 4 + byteCount;
    }
    public static int LengthOf(string? value)
    {
        if (value is null)
            return 1;
        else
            return LengthOf(value.AsSpan()) + 1;
    }
    public static int LengthOf(Guid value)
    {
        return 16;
    }
    #endregion Length

    #region Read
    public static bool ReadBool(Stream stream)
    {
        return ReadU8(stream) != 0;
    }
    public static bool? ReadNullableBool(Stream stream)
    {
        if (ReadBool(stream))
            return ReadBool(stream);
        return null;
    }
    public static byte ReadU8(Stream stream)
    {
        int read = stream.ReadByte();
        if (read < 0)
            throw new EndOfStreamException();
        return (byte)read;
    }
    public static byte? ReadNullableU8(Stream stream)
    {
        if (ReadBool(stream))
            return ReadU8(stream);
        return null;
    }
    public static sbyte ReadI8(Stream stream)
    {
        return (sbyte)ReadU8(stream);
    }
    public static sbyte? ReadNullableI8(Stream stream)
    {
        return (sbyte?)ReadNullableU8(stream);
    }
    public static ushort ReadU16(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[2];
        stream.ReadExactly(bytes);
        return (ushort)((bytes[0] << 8) | bytes[1]);
    }
    public static ushort? ReadNullableU16(Stream stream)
    {
        if (ReadBool(stream))
            return ReadU16(stream);
        return null;
    }
    public static short ReadI16(Stream stream)
    {
        return (short)ReadU16(stream);
    }
    public static short? ReadNullableI16(Stream stream)
    {
        return (short?)ReadNullableU16(stream);
    }
    public static uint ReadU32(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[4];
        stream.ReadExactly(bytes);
        return (uint)(
            (bytes[0] << 24) |
            (bytes[1] << 16) |
            (bytes[2] << 8) |
            bytes[3]);
    }
    public static uint? ReadNullableU32(Stream stream)
    {
        if (ReadBool(stream))
            return ReadU32(stream);
        return null;
    }
    public static int ReadI32(Stream stream)
    {
        return (int)ReadU32(stream);
    }
    public static int? ReadNullableI32(Stream stream)
    {
        return (int?)ReadNullableU32(stream);
    }
    public static ulong ReadU64(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[8];
        stream.ReadExactly(bytes);
        return (ulong)(
            (bytes[0] << 56) |
            (bytes[1] << 48) |
            (bytes[2] << 40) |
            (bytes[3] << 32) |
            (bytes[4] << 24) |
            (bytes[5] << 16) |
            (bytes[6] << 8) |
            bytes[7]);
    }
    public static ulong? ReadNullableU64(Stream stream)
    {
        if (ReadBool(stream))
            return ReadU64(stream);
        return null;
    }
    public static long ReadI64(Stream stream)
    {
        return (long)ReadU64(stream);
    }
    public static long? ReadNullableI64(Stream stream)
    {
        return (long?)ReadNullableU64(stream);
    }
    public static UInt128 ReadU128(Stream stream)
    {
        ulong upper = ReadU64(stream);
        ulong lower = ReadU64(stream);
        return new(upper, lower);
    }
    public static UInt128? ReadNullableU128(Stream stream)
    {
        if (ReadBool(stream))
            return ReadU128(stream);
        return null;
    }
    public static Int128 ReadI128(Stream stream)
    {
        return (Int128)ReadU128(stream);
    }
    public static Int128? ReadNullableI128(Stream stream)
    {
        return (Int128?)ReadNullableU128(stream);
    }
    public static Half ReadHalf(Stream stream)
    {
        return BitConverter.UInt16BitsToHalf(ReadU16(stream));
    }
    public static Half? ReadNullableHalf(Stream stream)
    {
        if (ReadBool(stream))
            return ReadHalf(stream);
        return null;
    }
    public static float ReadSingle(Stream stream)
    {
        return BitConverter.UInt32BitsToSingle(ReadU32(stream));
    }
    public static float? ReadNullableSingle(Stream stream)
    {
        if (ReadBool(stream))
            return ReadSingle(stream);
        return null;
    }
    public static double ReadDouble(Stream stream)
    {
        return BitConverter.UInt64BitsToDouble(ReadU64(stream));
    }
    public static double? ReadNullableDouble(Stream stream)
    {
        if (ReadBool(stream))
            return ReadDouble(stream);
        return null;
    }
    public static decimal ReadDecimal(Stream stream)
    {
        ReadOnlySpan<int> bits = [
            ReadI32(stream),
            ReadI32(stream),
            ReadI32(stream),
            ReadI32(stream)
            ];
        return new(bits);
    }
    public static decimal? ReadNullableDecimal(Stream stream)
    {
        if (ReadBool(stream))
            return ReadDecimal(stream);
        return null;
    }
    public static TimeSpan ReadTimeSpan(Stream stream)
    {
        return new(ReadI64(stream));
    }
    public static TimeSpan? ReadNullableTimeSpan(Stream stream)
    {
        if (ReadBool(stream))
            return ReadTimeSpan(stream);
        return null;
    }
    public static DateTime ReadDateTime(Stream stream)
    {
        Span<byte> bBuffer = stackalloc byte[40];
        Span<char> cBuffer = stackalloc char[40];
        int read = ReadByteArray(stream, bBuffer);
        read = Encoding.Latin1.GetChars(bBuffer[..read], cBuffer);
        return DateTime.Parse(cBuffer[..read]);
    }
    public static DateTime? ReadNullableDateTime(Stream stream)
    {
        if (ReadBool(stream))
            return ReadDateTime(stream);
        return null;
    }
    public static DateTimeOffset ReadDateTimeOffset(Stream stream)
    {
        Span<byte> bBuffer = stackalloc byte[40];
        Span<char> cBuffer = stackalloc char[40];
        int read = ReadByteArray(stream, bBuffer);
        read = Encoding.Latin1.GetChars(bBuffer[..read], cBuffer);
        return DateTimeOffset.Parse(cBuffer[..read]);
    }
    public static DateTimeOffset? ReadNullableDateTimeOffset(Stream stream)
    {
        if (ReadBool(stream))
            return ReadDateTimeOffset(stream);
        return null;
    }
    public static int ReadByteArray(Stream stream, Span<byte> buffer)
    {
        int length = ReadI32(stream);
        if (length <= buffer.Length)
        {
            stream.ReadExactly(buffer[..length]);
            return length;
        }
        else
        {
            stream.ReadExactly(buffer);
            stream.Skip(length - buffer.Length);
            return buffer.Length;
        }
    }
    public static MemoryBuffer<byte> ReadByteArrayAsMemoryBuffer(Stream stream)
    {
        int length = ReadI32(stream);
        MemoryBuffer<byte> result = new(length);
        stream.ReadExactly(result);
        return result;
    }
    public static byte[] ReadByteArray(Stream stream)
    {
        int length = ReadI32(stream);
        byte[] result = new byte[length];
        stream.ReadExactly(result);
        return result;
    }
    public static int ReadString(Stream stream, Span<char> buffer)
    {
        Decoder decoder = Encoding.UTF8.GetDecoder();
        int byteCount = ReadI32(stream);
        Span<byte> byteBuffer = stackalloc byte[Math.Min(byteCount, PAYLOAD_BUFFER_SIZE)];
        int charSaved = 0;
        int dataRemaining = byteCount;
        int bufferData = 0;
        bool completed = false;
        int charUsed = 0;
        while (dataRemaining > 0 || !completed)
        {
            if (dataRemaining > 0)
            {
                Span<byte> buf = byteBuffer[bufferData..];
                if (dataRemaining < buf.Length)
                    buf = buf[..dataRemaining];
                int read = stream.Read(buf);
                if (bufferData < byteBuffer.Length && read == 0 && dataRemaining != 0)
                    throw new EndOfStreamException();
                dataRemaining -= read;
                bufferData += read;
            }
            decoder.Convert(byteBuffer[..bufferData], buffer, false, out int byteUsed, out charUsed, out completed);
            if (byteUsed == 0 && charUsed == 0)
            {
                stream.Skip(dataRemaining);
                completed = true;
            }
            bufferData -= byteUsed;
            charSaved += charUsed;
            byteBuffer[byteUsed..].CopyTo(byteBuffer);
            buffer = buffer[charUsed..];
        }
        return charSaved;
    }
    public static string ReadString(Stream stream)
    {
        int byteCount = ReadI32(stream);
        if (byteCount <= PAYLOAD_BUFFER_SIZE)
        {
            Span<byte> byteBuffer = stackalloc byte[byteCount];
            stream.ReadExactly(byteBuffer);
            return Encoding.UTF8.GetString(byteBuffer);
        }
        else
        {
            Decoder decoder = Encoding.UTF8.GetDecoder();
            Span<byte> byteBuffer = stackalloc byte[PAYLOAD_BUFFER_SIZE];
            Span<char> charBuffer = stackalloc char[PAYLOAD_BUFFER_SIZE];
            StringBuilder sb = new();
            int dataRemaining = byteCount;
            int bufferData = 0;
            bool completed = false;
            int charUsed = 0;
            while (!completed)
            {
                if (dataRemaining > 0)
                {
                    Span<byte> buf = byteBuffer[bufferData..];
                    if (dataRemaining < buf.Length)
                        buf = buf[..dataRemaining];
                    int read = stream.Read(buf);
                    if (bufferData < byteBuffer.Length && read == 0 && dataRemaining != 0)
                        throw new EndOfStreamException();
                    dataRemaining -= read;
                    bufferData += read;
                }
                decoder.Convert(byteBuffer[..bufferData], charBuffer, false, out int byteUsed, out charUsed, out completed);
                bufferData -= byteUsed;
                byteBuffer[byteUsed..].CopyTo(byteBuffer);
                sb.Append(charBuffer[charUsed..]);
            }
            return sb.ToString();
        }
    }
    public static string? ReadNullableString(Stream stream)
    {
        if (ReadBool(stream))
            return ReadString(stream);
        return null;
    }
    public static Guid ReadGuid(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[16];
        stream.ReadExactly(buffer);
        return new(buffer, true);
    }
    #endregion Read
}

using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Positron.NetworkIoAPI
{
    public sealed class PositronNetworkReader
    {
        private readonly IPositronSerializer _complexDataSerializer;
        private readonly byte[] _buffer;
        private readonly byte[] _emptyBuffer;

        private int _pointerPosition = 0;

        /// <summary>
        /// Creates new reader
        /// </summary>
        /// <param name="complexDataSerializer">Serializer than is used for writing structs or classes into network</param>
        /// <param name="bufferSize">main buffer size in bytes. by default is 65536 (64 KB)</param>
        /// <exception cref="ArgumentNullException">occurs when complex serializer not passed</exception>
        public PositronNetworkReader(IPositronSerializer complexDataSerializer, int bufferSize = 64 * 1024)
        {
            _complexDataSerializer = complexDataSerializer ?? throw new ArgumentNullException($"{typeof(IPositronSerializer)} can`t be null");
            _buffer = new byte[bufferSize];
            _emptyBuffer = new byte[0];
        }

        /// <summary>
        /// Copies given buffer to reader
        /// </summary>
        /// <param name="sourceBuffer">Source bytes buffer</param>
        /// <exception cref="ArgumentException">occurs when given data too large and can`t be placed correctly</exception>
        public void AllocFrom(ReadOnlySpan<byte> sourceBuffer)
        {
            if (sourceBuffer.Length > _buffer.Length)
            {
                throw new ArgumentException("Source buffer can`t be larger than readers buffer");
            }

            FreeAll();

            sourceBuffer.CopyTo(_buffer.AsSpan(0, sourceBuffer.Length));
            _pointerPosition = sourceBuffer.Length - 1;
        }

        /// <summary>
        /// Eliminates data in reader
        /// </summary>
        public void FreeAll()
        {
            _buffer.AsSpan().Clear();
            _pointerPosition = 0;
        }

        /// <summary>
        /// Reads long value from internal buffer
        /// </summary>
        /// <returns>long (int64) value</returns>
        public long ReadLong() => BinaryPrimitives.ReadInt64BigEndian(Bfree(8));

        /// <summary>
        /// Reads int value from internal buffer
        /// </summary>
        /// <returns>int (int32) value</returns>
        public int ReadInt() => BinaryPrimitives.ReadInt32BigEndian(Bfree(4));

        /// <summary>
        /// Reads short value from internal buffer
        /// </summary>
        /// <returns>short (int16) value</returns>
        public short ReadShort() => BinaryPrimitives.ReadInt16BigEndian(Bfree(2));

        /// <summary>
        /// Reads ulong value from internal buffer
        /// </summary>
        /// <returns>ulong (uint64) value</returns>
        public ulong ReadUlong() => BinaryPrimitives.ReadUInt64BigEndian(Bfree(8));

        /// <summary>
        /// Reads uint value from internal buffer
        /// </summary>
        /// <returns>uint (uint32) value</returns>
        public uint ReadUint() => BinaryPrimitives.ReadUInt32BigEndian(Bfree(4));

        /// <summary>
        /// Reads ushort value from internal buffer
        /// </summary>
        /// <returns>ushort (uint16) value</returns>
        public ushort ReadUshort() => BinaryPrimitives.ReadUInt16BigEndian(Bfree(2));

        /// <summary>
        /// Reads byte value from internal buffer
        /// </summary>
        /// <returns>byte value</returns>
        public byte ReadByte() => Bfree(1)[0];

        /// <summary>
        /// Reads sbyte value from internal buffer
        /// </summary>
        /// <returns>sbyte (signed byte) value</returns>
        public sbyte ReadSbyte() => (sbyte)Bfree(1)[0];

        /// <summary>
        /// Reads bool value from internal buffer
        /// </summary>
        /// <returns>bool value</returns>
        public bool ReadBool() => Bfree(1)[0] == 1;

        /// <summary>
        /// Reads float value from internal buffer
        /// </summary>
        /// <returns>float (float32) value</returns>
        public float ReadFloat() => BitConverter.Int32BitsToSingle(ReadInt());

        /// <summary>
        /// Reads double value from internal buffer
        /// </summary>
        /// <returns>double (float64) value</returns>
        public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadLong());


        /// <summary>
        /// Reads piece of buffer by size header
        /// </summary>
        /// <returns>read only span of internal buffer`s bytes</returns>
        public ReadOnlySpan<byte> ReadBytes()
        {
            int size = ReadInt();

            if (size != 0)
            {
                return Bfree(size);
            }

            return _emptyBuffer.AsSpan();
        }

        /// <summary>
        /// Reads string (ALLOCATES)
        /// </summary>
        /// <returns>Allocates new string from bytes in the buffer</returns>
        public string ReadString()
        {
            int size = ReadInt();
            return Encoding.UTF8.GetString(Bfree(size));
        }

        // TODO: Read complex

        private ReadOnlySpan<byte> Bfree(int size)
        {
            int offset = size - 1;
            CheckSizeAndThrow(offset);
            _pointerPosition -= offset;
            return _buffer.AsSpan(_pointerPosition, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckSizeAndThrow(int size)
        {
            if (_pointerPosition - size < 0)
            {
                ThrowErr();
            }
        }

        [DoesNotReturn]
        private void ThrowErr() => throw new IndexOutOfRangeException("PositronNetworkReader overreaded, can`t read out of memory bounds");
    }
}
using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Positron.NetworkIoAPI
{
    public sealed class PositronNetworkWriter
    {
        private readonly IPositronSerializer _complexDataSerializer;
        private readonly byte[] _buffer;
        private readonly byte[] _tempComplexDataSerializeBuffer;

        private int _pointerPosition;

        /// <summary>
        /// All data containing in this writer. use it for sending via network
        /// </summary>
        public ReadOnlySpan<byte> Data => _buffer.AsSpan(0, _pointerPosition);

        /// <summary>
        /// All data in buffer as ReadOnlyMemory
        /// </summary>
        public ReadOnlyMemory<byte> Memory => _buffer.AsMemory(0, _pointerPosition);

        /// <summary>
        /// Creates new writer
        /// </summary>
        /// <param name="complexDataSerializer">Serializer than is used for writing structs or classes into network</param>
        /// <param name="bufferSize">main buffer size in bytes. by default is 65536 (64 KB)</param>
        /// <param name="tempBufferSize">buffer for serializing complex data. by default is 16384 (16 KB)</param>
        /// <exception cref="ArgumentNullException">occurs when complex serializer not passed</exception>
        public PositronNetworkWriter(IPositronSerializer complexDataSerializer, int bufferSize = 64 * 1024, int tempBufferSize = 16 * 1024)
        {
            _complexDataSerializer = complexDataSerializer ?? throw new ArgumentNullException($"{typeof(IPositronSerializer)} can`t be null");
            _buffer = new byte[bufferSize];
            _tempComplexDataSerializeBuffer = new byte[tempBufferSize];
        }

        /// <summary>
        /// Resets writer
        /// </summary>
        /// <param name="eraseDataFully">If setted TRUE runs loop than rewrites all remaining data with zeros</param>
        public void Clear(bool eraseDataFully = false)
        {
            if (eraseDataFully)
            {
                _buffer.AsSpan(0, _pointerPosition).Clear();
                _tempComplexDataSerializeBuffer.AsSpan().Clear();
            }

            _pointerPosition = 0;
        }

        /// <summary>
        /// Writes int64 (aliased in C# as long) to buffer
        /// </summary>
        /// <param name="toWrite">signed int64 value</param>
        /// <exception cref="IndexOutOfRangeException">Occurs when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteLong(long toWrite) => BinaryPrimitives.WriteInt64BigEndian(Balloc(8), toWrite);

        /// <summary>
        /// Writes int32 to buffer
        /// </summary>
        /// <param name="toWrite">signed int32 value</param>
        /// <exception cref="IndexOutOfRangeException">Occurs when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteInt(int toWrite) => BinaryPrimitives.WriteInt32BigEndian(Balloc(4), toWrite);

        /// <summary>
        /// Writes int16 (aliased in C# as short) to buffer
        /// </summary>
        /// <param name="toWrite">signed int16 value</param>
        /// <exception cref="IndexOutOfRangeException">Occurs when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteShort(short toWrite) => BinaryPrimitives.WriteInt16BigEndian(Balloc(2), toWrite);

        /// <summary>
        /// Writes uint64 (aliased in C# as ulong) to buffer
        /// </summary>
        /// <param name="toWrite">signed uint64 value</param>
        /// <exception cref="IndexOutOfRangeException">Occurs when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteUlong(ulong toWrite) => BinaryPrimitives.WriteUInt64BigEndian(Balloc(8), toWrite);

        /// <summary>
        /// Writes uint32 (aliased in C# as uint) to buffer
        /// </summary>
        /// <param name="toWrite">signed uint32 value</param>
        /// <exception cref="IndexOutOfRangeException">Occurs when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteUint(uint toWrite) => BinaryPrimitives.WriteUInt32BigEndian(Balloc(4), toWrite);

        /// <summary>
        /// Writes uint16 (aliased in C# as ushort) to buffer
        /// </summary>
        /// <param name="toWrite">signed uint16 value</param>
        /// <exception cref="IndexOutOfRangeException">Occurs when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteUshort(ushort toWrite) => BinaryPrimitives.WriteUInt16BigEndian(Balloc(2), toWrite);

        /// <summary>
        /// Writes one byte into buffer
        /// </summary>
        /// <param name="toWrite">byte value</param>
        public void WriteByte(byte toWrite) => Balloc(1)[0] = toWrite;

        /// <summary>
        /// Writes one signed byte into buffer
        /// </summary>
        /// <param name="toWrite">signed byte value</param>
        public void WriteSbyte(sbyte toWrite) => Balloc(1)[0] = (byte)toWrite;

        /// <summary>
        /// Writes bool as byte (0 or 1) to buffer
        /// </summary>
        /// <param name="toWrite">bool value</param>
        public void WriteBool(bool toWrite) => Balloc(1)[0] = (byte)(toWrite ? 1 : 0);

        /// <summary>
        /// Writes float into buffer
        /// </summary>
        /// <param name="toWrite">float (Single or float32) value</param>
        public void WriteFloat(float toWrite) => WriteInt(BitConverter.SingleToInt32Bits(toWrite));

        /// <summary>
        /// Writes double into buffer
        /// </summary>
        /// <param name="toWrite">double (Double or float64) value</param>
        public void WriteDouble(double toWrite) => WriteLong(BitConverter.DoubleToInt64Bits(toWrite));

        /// <summary>
        /// Copies raw binary data buffer into internal buffer
        /// </summary>
        /// <param name="data">binary span (can be passed as ordinary byte[] it automatically casts into ReadOnlySpan)</param>
        public void WriteBytes(ReadOnlySpan<byte> data)
        {
            WriteUint((uint)data.Length);

            if (data.Length != 0)
            {
                data.CopyTo(Balloc(data.Length));
            }
        }

        /// <summary>
        /// writes given string into internal buffer as bytes with UTF8 encoding
        /// </summary>
        /// <param name="toWrite"></param>
        public void WriteString(string toWrite)
        {
            if (string.IsNullOrEmpty(toWrite))
            {
                WriteInt(0);
                return;
            }

            int size = Encoding.UTF8.GetByteCount(toWrite);
            WriteInt(size);
            Encoding.UTF8.GetBytes(toWrite, Balloc(size));
        }

        /// <summary>
        /// Uses current network serializer to write data. 
        /// Please make sure your data is marked with specific attributes (for MessagePack) or Schemas valid (for Protobuf)
        /// YOU MUST MAKE SURE YOU ARE RUN CODE GEN FOR SERIALIZERS!!!
        /// </summary>
        /// <typeparam name="T">any ref type or ordinary struct</typeparam>
        /// <param name="complex">data instance or copy</param>
        /// <exception cref="InternalBufferOverflowException">Occurs when can`t put data to buffer (complex data intermediate buffer) correctly because serialized representation larger than setted size</exception>
        /// <exception cref="InternalBufferOverflowException">Occurs when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteComplexObject<T>(T complex)
        {
            if (complex == null)
            {
                throw new ArgumentNullException();
            }

            const int SIZE_HEADER = 4;
            int bytesWritten;

            try
            {
                bytesWritten = _complexDataSerializer.Serialize(complex, _tempComplexDataSerializeBuffer.AsSpan());
            }
            catch(Exception e) 
            {
                throw new InternalBufferOverflowException("Can`t write complex data because internal serialize buffer is overloaded. extend it at creation time. Serializer E:", e);
            }

            if (_pointerPosition + SIZE_HEADER + bytesWritten > _buffer.Length)
            {
                throw new InternalBufferOverflowException("PositronNetworkWriter overloaded, please clear it!");
            }

            WriteUint((uint)bytesWritten);
            _tempComplexDataSerializeBuffer.AsSpan(0, bytesWritten).CopyTo(_buffer.AsSpan(_pointerPosition, bytesWritten));
            _pointerPosition += bytesWritten;
        }

        private Span<byte> Balloc(int size)
        {
            CheckSizeAndThrow(size);
            Span<byte> bufferSpan = _buffer.AsSpan(_pointerPosition, size);
            _pointerPosition += size;

            return bufferSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckSizeAndThrow(int additionalSize)
        {
            if (_pointerPosition + additionalSize > _buffer.Length)
            {
                ThrowErr();
            }
        }

        [DoesNotReturn]
        private void ThrowErr() => throw new InternalBufferOverflowException("PositronNetworkWriter overloaded, please clear it!");
    }
}
using Positron.Client.Interfaces;
using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

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
        public ReadOnlySpan<byte> Data => new ReadOnlySpan<byte>(_buffer).Slice(0, _pointerPosition);

        /// <summary>
        /// Creates new writer
        /// </summary>
        /// <param name="complexDataSerializer">Serializer tha used for writing structs or classes into network</param>
        /// <param name="bufferSize">main buffer size in bytes. by default is 65536 (64 KB)</param>
        /// <param name="tempBufferSize">buffer for serializing complex data. by default is 16384 (16 KB)</param>
        public PositronNetworkWriter(IPositronSerializer complexDataSerializer, int bufferSize = 64 * 1024, int tempBufferSize = 16 * 1024)
        {
            _complexDataSerializer = complexDataSerializer ?? throw new ArgumentNullException($"{typeof(IPositronSerializer)} can`t be null");
            _buffer = new byte[bufferSize];
            _tempComplexDataSerializeBuffer = new byte[tempBufferSize];
        }

        /// <summary>
        /// Resets writter
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
        /// <exception cref="IndexOutOfRangeException">Occures when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteLong(long toWrite)
        {
            const int VAR_SIZE = 8;
            CheckSizeAndThrow(VAR_SIZE);

            Span<byte> bufferSpan = _buffer;
            BinaryPrimitives.WriteInt64BigEndian(bufferSpan.Slice(_pointerPosition, VAR_SIZE), toWrite);

            MovePtr(VAR_SIZE);
        }

        /// <summary>
        /// Writes int32 to buffer
        /// </summary>
        /// <param name="toWrite">signed int32 value</param>
        /// <exception cref="IndexOutOfRangeException">Occures when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteInt(int toWrite)
        {
            const int VAR_SIZE = 4;
            CheckSizeAndThrow(VAR_SIZE);

            Span<byte> bufferSpan = _buffer;
            BinaryPrimitives.WriteInt32BigEndian(bufferSpan.Slice(_pointerPosition, VAR_SIZE), toWrite);

            MovePtr(VAR_SIZE);
        }

        /// <summary>
        /// Writes int16 (aliased in C# as short) to buffer
        /// </summary>
        /// <param name="toWrite">signed int16 value</param>
        /// <exception cref="IndexOutOfRangeException">Occures when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteShort(short toWrite)
        {
            const int VAR_SIZE = 2;
            CheckSizeAndThrow(VAR_SIZE);

            Span<byte> bufferSpan = _buffer;
            BinaryPrimitives.WriteInt16BigEndian(bufferSpan.Slice(_pointerPosition, VAR_SIZE), toWrite);

            MovePtr(VAR_SIZE);
        }

        /// <summary>
        /// Writes uint64 (aliased in C# as ulong) to buffer
        /// </summary>
        /// <param name="toWrite">signed uint64 value</param>
        /// <exception cref="IndexOutOfRangeException">Occures when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteUlong(ulong toWrite)
        {
            const int VAR_SIZE = 8;
            CheckSizeAndThrow(VAR_SIZE);

            Span<byte> bufferSpan = _buffer;
            BinaryPrimitives.WriteUInt64BigEndian(bufferSpan.Slice(_pointerPosition, VAR_SIZE), toWrite);

            MovePtr(VAR_SIZE);
        }

        /// <summary>
        /// Writes uint32 (aliased in C# as uint) to buffer
        /// </summary>
        /// <param name="toWrite">signed uint32 value</param>
        /// <exception cref="IndexOutOfRangeException">Occures when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteUint(uint toWrite)
        {
            const int VAR_SIZE = 4;
            CheckSizeAndThrow(VAR_SIZE);

            Span<byte> bufferSpan = _buffer;
            BinaryPrimitives.WriteUInt32BigEndian(bufferSpan.Slice(_pointerPosition, VAR_SIZE), toWrite);

            MovePtr(VAR_SIZE);
        }

        /// <summary>
        /// Writes uint16 (aliased in C# as ushort) to buffer
        /// </summary>
        /// <param name="toWrite">signed uint16 value</param>
        /// <exception cref="IndexOutOfRangeException">Occures when new serialized data larger than remaining size in writer`s buffer</exception>
        public void WriteUshort(ushort toWrite)
        {
            const int VAR_SIZE = 2;
            CheckSizeAndThrow(VAR_SIZE);

            Span<byte> bufferSpan = _buffer;
            BinaryPrimitives.WriteUInt16BigEndian(bufferSpan.Slice(_pointerPosition, VAR_SIZE), toWrite);

            MovePtr(VAR_SIZE);
        }

        public void WriteBool(bool toWrite)
        {
            const int VAR_SIZE = 1;
            CheckSizeAndThrow(VAR_SIZE);

            Span<byte> bufferSpan = _buffer;
            bufferSpan.Slice(_pointerPosition, VAR_SIZE)[0] = (byte)(toWrite ? 1 : 0);

            MovePtr(VAR_SIZE);
        }

        /// <summary>
        /// Uses current network serializer to write data. 
        /// Please make sure your data is marked with specific attributes (for MessagePack) or Schemas valid (for Protobuf)
        /// YOU MUST MAKE SURE YOU ARE RUN CODE GEN FOR SERIALIZERS!!!
        /// </summary>
        /// <typeparam name="T">any ref type or ordinary struct</typeparam>
        /// <param name="complex">data instance or copy</param>
        /// <exception cref="InternalBufferOverflowException">Occures when new serialized data larger than remaining size in writer`s buffer</exception>
        /// <exception cref="InternalBufferOverflowException">Occures when can`t put data to buffer correctly because serialized representation larger than 16 KB</exception>
        public void Write<T>(T complex)
        {
            const int KB16 = 16 * 1024; 

            Span<byte> tempBuffer = _tempComplexDataSerializeBuffer;
            int bytesWriten = _complexDataSerializer.Serialize(complex, _tempComplexDataSerializeBuffer);

            if (_pointerPosition + 2 + bytesWriten > _buffer.Length)
            {
                throw new InternalBufferOverflowException("PositronNetworkWriter overloaded, please clear it!");
            }

            if (bytesWriten > KB16)
            {
                throw new InternalBufferOverflowException("PositronNetworkWriter can`t put data to buffer correctly because serialized representation larger than 16 KB");
            }

            WriteUshort((ushort)bytesWriten);

            Span<byte> bufferSpan = _buffer;
            tempBuffer.CopyTo(bufferSpan.Slice(_pointerPosition, bytesWriten));

            MovePtr(bytesWriten);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MovePtr(int size)
        {
            _pointerPosition += size;
        }
    }
}
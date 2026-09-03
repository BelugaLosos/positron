using System;
using System.Text;

namespace Positron.BytesArena
{
    /// <summary>
    /// Custom memory manager for hot parts of system
    /// </summary>
    public sealed class TransientArena
    {
        private byte[] _buffer;
        private int _writePtr;

        /// <summary>
        /// Get`s all available data
        /// </summary>
        public ReadOnlySpan<byte> Data => _buffer.AsSpan(0, _writePtr);

        public TransientArena()
        {
            _buffer = new byte[64 * 1024]; //64 KB
        }

        /// <summary>
        /// Clones arena to internal buffer
        /// </summary>
        /// <param name="data">source arena</param>
        public void CloneFrom(ReadOnlyMemory<byte> data)
        {
            Flush();
            Alloc(data.Span, out int _);
        }

        /// <summary>
        /// Sequentically allocates memory in internal buffer without any headers
        /// </summary>
        /// <param name="data">raw bytes</param>
        /// <returns>pointer to internal buffer</returns>
        public int Alloc(ReadOnlySpan<byte> data, out int len)
        {
            int size = data.Length;
            int ptr = _writePtr;

            CheckSizeAndGrow(size);
            data.CopyTo(_buffer.AsSpan(_writePtr, size));
            _writePtr += size;

            len = size;
            return ptr;
        }

        /// <summary>
        /// Reads data from internal buffer
        /// </summary>
        /// <param name="ptr">offset (pointer) at buffer</param>
        /// <param name="len">lengths to read</param>
        /// <returns>Link to internals data (NOT A COPY!)</returns>
        /// <exception cref="IndexOutOfRangeException">Throwed when you try to read outside of physically allocated buffer</exception>
        public Span<byte> Read(uint ptr, uint len)
        {
            if ((ptr + len) > _buffer.Length)
            {
                throw new IndexOutOfRangeException($"Unable to read memory at {ptr} {len} {_buffer.Length}");
            }

            return _buffer.AsSpan((int)ptr, (int)len);
        }

        /// <summary>
        /// Reads data from internal buffer (as memory)
        /// </summary>
        /// <param name="ptr">offset (pointer) at buffer</param>
        /// <param name="len">lengths to read</param>
        /// <returns>Link to internals data (NOT A COPY!) as memory</returns>
        /// <exception cref="IndexOutOfRangeException">Throwed when you try to read outside of physically allocated buffer</exception>
        public Memory<byte> ReadAsMem(uint ptr, uint len)
        {
            if ((ptr + len) > _buffer.Length)
            {
                throw new IndexOutOfRangeException($"Unable to read memory at {ptr} {len} {_buffer.Length}");
            }

            return _buffer.AsMemory((int)ptr, (int)len);
        }

        /// <summary>
        /// All data GONE after that operation
        /// </summary>
        public void Flush() 
        {
            _buffer.AsSpan().Clear();
            _writePtr = 0;
        }

        private void CheckSizeAndGrow(int size)
        {
            if ((_writePtr + size) >= _buffer.Length)
            {
                byte[] doubled = new byte[_writePtr + (size * 2)];
                _buffer.AsSpan().CopyTo(doubled.AsSpan());
                _buffer = doubled;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"PTR: {_writePtr}");
            sb.AppendLine("DAT: ");

            for (int i = 0; i < _writePtr; i++)
            {
                sb.Append(' ');
                sb.Append(_buffer[i]);
                sb.Append(' ');
            }
            
            return sb.ToString();
        }
    }
}
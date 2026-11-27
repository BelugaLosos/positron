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
        /// <summary>
        /// Actual size of arena
        /// </summary>
        public int EndOffset => _writePtr;

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
        /// <returns>Link to internals data (NOT A COPY!) as memory technically this data can live untill rewrite but after Flush() reading this segment is UB</returns>
        /// <exception cref="IndexOutOfRangeException">Throwed when you try to read outside of physically allocated buffer</exception>
        public Span<byte> Read(uint ptr, uint len)
        {
            if (((ulong)ptr + len) > (ulong)_writePtr)
            {
                throw new IndexOutOfRangeException($"Unable to read memory at {ptr} {len} {_writePtr}");
            }

            return _buffer.AsSpan((int)ptr, (int)len);
        }

        /// <summary>
        /// Reads data from internal buffer (as memory)
        /// </summary>
        /// <param name="ptr">offset (pointer) at buffer</param>
        /// <param name="len">lengths to read</param>
        /// <returns>Link to internals data (NOT A COPY!) as memory technically this data can live untill rewrite but after Flush() reading this segment is UB</returns>
        /// <exception cref="IndexOutOfRangeException">Throwed when you try to read outside of physically allocated buffer</exception>
        public ReadOnlyMemory<byte> ReadAsMem(uint ptr, uint len)
        {
            if (((ulong)ptr + len) > (ulong)_writePtr)
            {
                throw new IndexOutOfRangeException($"Unable to read memory at {ptr} {len} {_writePtr}");
            }

            return _buffer.AsMemory((int)ptr, (int)len);
        }

        /// <summary>
        /// All data GONE (virtualy, technically they stay there forever untill rewriting) after that operation
        /// </summary>
        public void Flush() 
        {
            _writePtr = 0;
        }

        private void CheckSizeAndGrow(int size)
        {
            int required = _writePtr + size;
            if (required > _buffer.Length)
            {
                byte[] doubled = new byte[Math.Max(_buffer.Length * 2, required)];
                _buffer.AsSpan(0, _writePtr).CopyTo(doubled);
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
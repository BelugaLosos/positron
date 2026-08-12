using System.Collections.Generic;
using Positron.Client.Interfaces;
using System;

namespace Positron.NetworkIoAPI
{
    public sealed class PositronNetworkIoPool
    {
        private readonly Stack<PositronNetworkWriter> _writersPool;
        private readonly Stack<PositronNetworkReader> _readersPool;

        private const int POOL_SIZE = 64;

        public PositronNetworkIoPool(IPositronSerializer serializer)
        {
            _writersPool = new();
            _readersPool = new();

            for (int i = 0; i < POOL_SIZE; i++)
            {
                _writersPool.Push(new(serializer));
            }

            for (int i = 0; i < POOL_SIZE; i++)
            {
                _readersPool.Push(new(serializer));
            }
        }

        public PositronNetworkWriter GetWriter()
        {
            if (_writersPool.Count == 0)
            {
                throw new InvalidOperationException("No writers left");
            }

            return _writersPool.Pop();
        }

        public PositronNetworkReader GetReader()
        {
            if (_readersPool.Count == 0)
            {
                throw new InvalidOperationException("No readers left");
            }

            return _readersPool.Pop();
        }

        public void PutWriter(PositronNetworkWriter writer)
        {
            writer.Clear(true);
            _writersPool.Push(writer);
        }

        public void PutReader(PositronNetworkReader reader)
        {
            reader.FreeAll();
            _readersPool.Push(reader);
        }
    }
}
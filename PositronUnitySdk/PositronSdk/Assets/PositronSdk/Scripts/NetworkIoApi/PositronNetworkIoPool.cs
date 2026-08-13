using System.Collections.Generic;
using Positron.Client.Interfaces;
using System;

namespace Positron.NetworkIoAPI
{
    public sealed class PositronNetworkIoPool
    {
        private int _writersGetted;
        private int _writersPutted;

        private int _readersGetted;
        private int _readersPutted;

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

            _writersGetted++;
            return _writersPool.Pop();
        }

        public PositronNetworkReader GetReader()
        {
            if (_readersPool.Count == 0)
            {
                throw new InvalidOperationException("No readers left");
            }

            _readersGetted++;
            return _readersPool.Pop();
        }

        public void PutWriter(PositronNetworkWriter writer)
        {
            _writersPutted++;

            writer.Clear(true);
            _writersPool.Push(writer);
        }

        public void PutReader(PositronNetworkReader reader)
        {
            _readersPutted++;

            reader.FreeAll();
            _readersPool.Push(reader);
        }

        public NetworkIoPoolStats GetStats() => new NetworkIoPoolStats(_writersGetted, _writersPutted, _readersGetted, _readersPutted);
    }

    public struct NetworkIoPoolStats
    {
        public int WritersGetted { get; private set; }
        public int WritersPutted { get; private set; }

        public int ReadersGetted { get; private set; }
        public int ReadersPutted { get; private set; }

        public int ActiveWriters { get; private set; }
        public int ActiveReaders { get; private set; }

        public NetworkIoPoolStats(int wg, int wp, int rg, int rp)
        {
            WritersGetted = wg; 
            WritersPutted = wp;

            ReadersGetted = rg; 
            ReadersPutted = rp;

            ActiveWriters = wg - wp;
            ActiveReaders = rg - rp;
        }
    }
}
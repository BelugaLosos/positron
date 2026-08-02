using Positron.Client.ConstantHolders;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;
using Positron.NetworkIoAPI;
using System;

namespace Positron.Client.Handlers
{
    public class GameTickHandler : IPositronHandler, IPositronObservableHandler<GameTickDataAndMeta>
    {
        private IPositronClient _client;
        private PositronNetworkReader _reader;

        public EventTypes MessageType => EventTypes.TICK;

        public event Action<GameTickDataAndMeta> callback;

        public void Init(IPositronClient client)
        {
            _client = client;
            _reader = new(_client.Serializer);
        }

        public void Dispose() { }

        public void Handle(ReadOnlyMemory<byte> packet)
        { 
            _reader.AllocFrom(packet);

            GameTickPacket meta = _reader.ReadComplex<GameTickPacket>();
            ReadOnlyMemory<byte> valuesArena = _reader.ReadBytes();
            ReadOnlyMemory<byte> rpcsArena = _reader.ReadBytes();

            callback?.Invoke(new(meta, valuesArena, rpcsArena));
        }
    }
}
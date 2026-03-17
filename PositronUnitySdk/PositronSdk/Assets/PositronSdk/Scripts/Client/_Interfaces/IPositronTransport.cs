using Cysharp.Threading.Tasks;
using Positron.Client.Settings;
using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronTransport
    {
        UniTask Connect(PositronSettings settings);
        UniTask Disconnect();
        void Send(Span<byte> buffer, byte type, bool isReliable);
        event Action<byte[]> onRawMessage;
    }
}
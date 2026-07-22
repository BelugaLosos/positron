using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronSerializer
    {
        void Init();
        int Serialize<T>(T data, Span<byte> destionation);
        T Deserialize<T>(ReadOnlyMemory<byte> data);
    }
}
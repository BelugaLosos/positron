using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronSerializer
    {
        void Init();
        Span<byte> Serialize<T>(T data);
        T Deserialize<T>(Span<byte> data);
    }
}
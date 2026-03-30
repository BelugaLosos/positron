using System;

namespace Positron.Client.Interfaces
{
    public interface IPositronObservableHandler<T> where T : struct
    {
        event Action<T> callback;
    }
}
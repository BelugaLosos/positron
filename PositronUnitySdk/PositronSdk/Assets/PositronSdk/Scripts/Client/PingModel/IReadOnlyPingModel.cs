using System;

namespace Positron.Client.Ping
{
    public interface IReadOnlyPingModel
    {
        int LatencyMs { get; }
        event Action estimated;
    }
}
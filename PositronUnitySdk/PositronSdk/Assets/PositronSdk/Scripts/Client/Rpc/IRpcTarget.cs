using Positron.NetworkIoAPI;

namespace Positron.Client.Rpc
{
    public interface IRpcTarget
    {
        bool IsSuitableTargetFor(string name);
        void Call(string name, PositronNetworkReader reader);
    }
}
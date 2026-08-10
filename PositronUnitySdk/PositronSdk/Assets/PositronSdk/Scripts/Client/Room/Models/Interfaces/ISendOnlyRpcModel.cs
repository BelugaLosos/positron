using Positron.Client.ConstantHolders;
using Positron.Client.Rpc;
using Positron.NetworkIoAPI;

namespace Positron.Client.Room.Models.Interfaces
{
    public interface ISendOnlyRpcModel
    {
        void SendRpcToServer(IRpcTarget obj, string methodName, uint specifiedTargetClient, bool hasSpecifiedTarget, RpcTargets targets, PositronNetworkWriter writer);
    }
}
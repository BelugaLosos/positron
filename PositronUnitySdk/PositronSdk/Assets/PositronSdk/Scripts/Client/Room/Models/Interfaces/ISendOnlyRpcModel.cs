using Positron.Client.ConstantHolders;
using Positron.Client.Rpc;
using Positron.NetworkIoAPI;
using UnityEngine;

namespace Positron.Client.Room.Models.Interfaces
{
    public interface ISendOnlyRpcModel
    {
        bool IsInRollbufferMode { get; }
        void SendRpcToServer(IRpcTarget obj, GameObject bindedGameObject, ulong methodName, uint specifiedTargetClient, bool hasSpecifiedTarget, RpcTargets targets, PositronNetworkWriter writer);
    }
}
using Positron.BytesArena;
using Positron.Client.ConstantHolders;
using Positron.Client.GameEntities;
using Positron.Client.Mono;
using Positron.NetworkIoAPI;
using Positron.Utility;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class RpcsModel : IDisposable
    {
        private uint _selfClientId;

        private readonly PooledDynamicArraySegment<RpcCall> _currentCallBuffer = new(128);

        private NetworkGameObjectsModel _gameObjectsModel;
        private TransientArena _incomingDataArena = new();
        private TransientArena _outgoingDataArena = new();

        public void Dispose()
        {
            _selfClientId = 0;
            _currentCallBuffer.Dispose();
        }

        public void Init(uint selfClientId, NetworkGameObjectsModel objectsMode)
        {
            _selfClientId = selfClientId;
            _gameObjectsModel = objectsMode;
        }

        public void ProcessServerRpcEvents(ArraySegment<RpcCall> calls, ReadOnlyMemory<byte> arena)
        {
            _incomingDataArena.CloneFrom(arena);

            foreach (RpcCall call in calls)
            {
                Call(call);
            }
        }

        public void SendRpcToServer(PositronNetworkIdentity obj, string methodName, uint specifiedTargetClient, RpcTargets targets, PositronNetworkWriter writer)
        {
            //GIANT TODOOOS
            //Get all ids
            //Convert method name to id
            //Validate and rewrite soecified target client according to targets param
            //write to arena and PUT writter back

            //AND MOST OF LOGIC MUST BE CODE GENERATED HERE!
        }

        private void Call(RpcCall call)
        {
            RpcTargets target = (RpcTargets)call.Type;

            switch (target)
            {
                case RpcTargets.RPC_ALL | RpcTargets.RPC_ALL_CACHED:
                    RouteRpcAll(call);
                    break;
                case RpcTargets.RPC_OTHERS | RpcTargets.RPC_OTHERS_CACHED:
                    RouteRpcOthers(call);
                    break;
                case RpcTargets.RPC_TARGET | RpcTargets.RPC_TARGET_CACHED:
                    RouteRpcTarget(call);
                    break;
                default: 
                    throw new ArgumentException($"Unsupported target {target}");
            }
        }

        private void RouteRpcAll(RpcCall call) => CallRpcLocal(call);

        private void RouteRpcOthers(RpcCall call)
        {
            uint excludingTargetClient = call.TargetClientId;

            if (excludingTargetClient == _selfClientId)
            {
                return;
            }

            CallRpcLocal(call);
        }

        private void RouteRpcTarget(RpcCall call)
        {
            if (call.TargetClientId != _selfClientId)
            {
                return;
            }

            CallRpcLocal(call);
        }

        private void CallRpcLocal(RpcCall call)
        {
            PositronNetworkIdentity obj = _gameObjectsModel.GetObjectByIds(call.ObjectId, call.SubObjectId);
            ReadOnlySpan<byte> data = _incomingDataArena.Read(call.ArenaLen, call.ArenaLen);
            
            // GIANT TODOOOOS
            // getting all IRpc from this object and cache it
            // reversive mapping from method id to string name
            // check IRpc suitable by method for EACH IRpc
            // if is it suitable call it... if NOT pass througth and get another IRpc
        }

        public ArraySegment<RpcCall> GetCurrentDelta() => _currentCallBuffer.ToArray();
        public ReadOnlySpan<byte> GetArena() => _outgoingDataArena.Data;
        public void ClearDelta()
        {
            _outgoingDataArena.Flush();
            _currentCallBuffer.Clear();
        }
    }
}
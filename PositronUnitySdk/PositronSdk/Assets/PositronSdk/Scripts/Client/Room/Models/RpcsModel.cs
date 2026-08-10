using Positron.BytesArena;
using Positron.Client.ConstantHolders;
using Positron.Client.GameEntities;
using Positron.Client.Mono;
using Positron.Client.Room.Models.Interfaces;
using Positron.Client.Rpc;
using Positron.NetworkIoAPI;
using Positron.Utility;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class RpcsModel : IDisposable, ISendOnlyRpcModel
    {
        private uint _selfClientId;

        private readonly PooledDynamicArraySegment<RpcCall> _currentCallBuffer = new(128);

        private IReadOnlyGameObjectsModel _gameObjectsModel;
        private TransientArena _incomingDataArena = new();
        private TransientArena _outgoingDataArena = new();

        public void Dispose()
        {
            _selfClientId = 0;
            _currentCallBuffer.Dispose();
        }

        public void Init(uint selfClientId, IReadOnlyGameObjectsModel objectsMode)
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

        public void SendRpcToServer(IRpcTarget obj, string methodName, uint specifiedTargetClient, bool hasSpecifiedTarget, RpcTargets targets, PositronNetworkWriter writer)
        {
            if (targets == RpcTargets.RPC_TARGET && !hasSpecifiedTarget)
            {
                throw new ArgumentException("Can`t call RPC_TARGET with no specified target argument (declare an argument like 'uint targetClientId')");
            }

            //GIANT TODOOOS
            //put a writer to pool
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
            // rent a reader
            // getting all IRpc from this object and cache it
            // reversive mapping from method id to string name
            // check IRpc suitable by method for EACH IRpc
            // if is it suitable call it... if NOT pass througth and get another IRpc

            // if not found put reader to pool and log error
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
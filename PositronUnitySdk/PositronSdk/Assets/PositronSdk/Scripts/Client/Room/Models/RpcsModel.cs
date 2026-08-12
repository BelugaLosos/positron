using Positron.BytesArena;
using Positron.Client.ConstantHolders;
using Positron.Client.GameEntities;
using Positron.Client.Mono;
using Positron.Client.Room.Models.Interfaces;
using Positron.Client.Rpc;
using Positron.Client.Settings;
using Positron.NetworkIoAPI;
using Positron.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Positron.Client.Room.Models
{
    public sealed class RpcsModel : IDisposable, ISendOnlyRpcModel
    {
        private uint _selfClientId;

        private readonly PooledDynamicArraySegment<RpcCall> _currentCallBuffer = new(128);

        private readonly Dictionary<IRpcTarget, PositronNetworkIdentity> _rpcToObj = new();
        private readonly Dictionary<ulong, ushort> _hashToIdx = new();
        private readonly ulong[] _idxToHash;

        private IReadOnlyGameObjectsModel _gameObjectsModel;
        private TransientArena _incomingDataArena = new();
        private TransientArena _outgoingDataArena = new();

        public RpcsModel(PositronSettings settings) 
        {
            _idxToHash = new ulong[settings.RpcMethodsNames.Length];

            for (int i = 0; i < settings.RpcMethodsNames.Length; i++)
            {
                ulong nameHash = settings.RpcMethodsNames[i];

                _hashToIdx.Add(nameHash, (ushort)i);
                _idxToHash[i] = nameHash;
            }
        }

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

        public void SendRpcToServer(IRpcTarget obj, GameObject bindedGameObject, ulong methodName, uint specifiedTargetClient, bool hasSpecifiedTarget, RpcTargets targets, PositronNetworkWriter writer)
        {
            if (targets == RpcTargets.RPC_TARGET && !hasSpecifiedTarget)
            {
                throw new ArgumentException("Can`t call RPC_TARGET with no specified target argument (declare an argument like 'uint targetClientId')");
            }

            if (!_rpcToObj.TryGetValue(obj, out PositronNetworkIdentity identity))
            {
                identity = bindedGameObject.GetComponent<PositronNetworkIdentity>();
                _rpcToObj.Add(obj, identity);
            }

            RpcCall rpcCallMeta = new();

            int ptr = _outgoingDataArena.Alloc(writer.Data, out int len);
            PositronFacade.NetworkIoPool.PutWriter(writer);

            rpcCallMeta.ArenaPtr = (uint)ptr;
            rpcCallMeta.ArenaLen = (uint)len;
            rpcCallMeta.TargetClientId = hasSpecifiedTarget ? specifiedTargetClient : _selfClientId;
            rpcCallMeta.ObjectId = identity.ObjectId;
            rpcCallMeta.MethodId = _hashToIdx[methodName];
            rpcCallMeta.SubObjectId = identity.SubObjectId;
            rpcCallMeta.Type = (byte)targets;

            _currentCallBuffer.Add(rpcCallMeta);
        }

        private void Call(RpcCall call)
        {
            RpcTargets target = (RpcTargets)call.Type;

            if (target == RpcTargets.RPC_ALL || target == RpcTargets.RPC_ALL_CACHED || target == RpcTargets.RPC_OTHERS || target == RpcTargets.RPC_OTHERS_CACHED)
            {
                RouteRpcWide(call);
            }
            else if (target == RpcTargets.RPC_TARGET || target == RpcTargets.RPC_TARGET_CACHED)
            {
                RouteRpcTarget(call);
            }
            else
            {
                throw new ArgumentException($"Unsupported target {target}");
            }
        }

        private void RouteRpcWide(RpcCall call)
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
            PositronNetworkReader reader = PositronFacade.NetworkIoPool.GetReader();
            ReadOnlySpan<byte> data = _incomingDataArena.Read(call.ArenaPtr, call.ArenaLen);
            reader.AllocFrom(data);

            PositronNetworkIdentity identity = _gameObjectsModel.GetObjectByIds(call.ObjectId, call.SubObjectId);

            IRpcTarget[] observedTargets = identity.GetObservedRpcTargets();
            ulong nameHash = _idxToHash[call.MethodId];
            bool founded = false;

            foreach (IRpcTarget rpcTarget in observedTargets)
            {
                if (rpcTarget == null)
                {
                    continue;
                }

                if (rpcTarget.IsSuitableTargetFor(nameHash))
                {
                    rpcTarget.Call(nameHash, reader);
                    founded = true;
                    break;
                }
            }

            if (!founded)
            {
                PositronFacade.NetworkIoPool.PutReader(reader);
                Debug.LogError($"Critical positron error -> unable to find any rpc at object {identity}", identity.gameObject);
            }
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
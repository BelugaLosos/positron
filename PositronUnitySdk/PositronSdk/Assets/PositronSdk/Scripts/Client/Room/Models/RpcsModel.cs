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
using System.Buffers.Binary;
using System.Collections.Generic;
using UnityEngine;

namespace Positron.Client.Room.Models
{
    public sealed class RpcsModel : IDisposable, ISendOnlyRpcModel
    {
        private uint _selfClientId;

        private readonly PooledDynamicArraySegment<RpcCall> _currentCallBuffer = new(128);
        private readonly Dictionary<PositronNetworkIdentity, DellayedCall> _dellayedCalls = new(24);

        private readonly Dictionary<IRpcTarget, PositronNetworkIdentity> _rpcToObj = new(256);
        private readonly Dictionary<ulong, ushort> _hashToIdx = new(256);
        private readonly ulong[] _idxToHash;

        private IReadOnlyGameObjectsModel _gameObjectsModel;
        private TransientArena _incomingDataArena = new();
        private TransientArena _outgoingDataArena = new();

        private readonly byte[] _flag = new byte[1];
        private readonly byte[] _creationIdSection = new byte[2];

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

            _gameObjectsModel.localObjectGoingRemove -= OnObjectRemove;
            _gameObjectsModel.localObjectRemotelyInitedSuccessfully -= OnLocalTargetInitedSuccesfully;
        }

        public void Init(uint selfClientId, IReadOnlyGameObjectsModel objectsMode)
        {
            _selfClientId = selfClientId;
            _gameObjectsModel = objectsMode;

            _gameObjectsModel.localObjectGoingRemove += OnObjectRemove;
            _gameObjectsModel.localObjectRemotelyInitedSuccessfully += OnLocalTargetInitedSuccesfully;
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
                PositronFacade.NetworkIoPool.PutWriter(writer);
                throw new ArgumentException("Can`t call RPC_TARGET with no specified target argument (declare an argument like 'uint targetClientId')");
            }

            if (!_rpcToObj.TryGetValue(obj, out PositronNetworkIdentity identity))
            {
                identity = bindedGameObject.GetComponent<PositronNetworkIdentity>();
                _rpcToObj.Add(obj, identity);
            }

            if (!identity.IsObjectFullyAvailable)
            {
                PositronFacade.NetworkIoPool.PutWriter(writer);
                throw new Exception("Unable to call RPC while object is not available yet. are you trying Awake method? Thats BAD! Use INetworkAwakeble / INetworkDestructable instead");
            }

            bool needToWriteCreationId = false;
            int len = 0;

            _flag[0] = 0;
            _creationIdSection[0] = 0;
            _creationIdSection[1] = 0;

            if (!identity.IsFullyInitialized)
            {
                if (_gameObjectsModel.HasObjectInCreationDeltaNow(identity.CreationId))
                {
                    needToWriteCreationId = true;

                    _flag[0] = 1;
                    BinaryPrimitives.WriteUInt16BigEndian(_creationIdSection, identity.CreationId);
                }
                else
                {
                    _dellayedCalls.Add(identity, new(obj, bindedGameObject, methodName, specifiedTargetClient, hasSpecifiedTarget, targets, writer));
                    return;
                }
            }

            int ptr = _outgoingDataArena.Alloc(_flag, out int _);
            len++;

            if (needToWriteCreationId)
            {
                _outgoingDataArena.Alloc(_creationIdSection, out int _);
                len += 2;
            }

            _outgoingDataArena.Alloc(writer.Data, out int customLen);
            len += customLen;

            PositronFacade.NetworkIoPool.PutWriter(writer);

            RpcCall rpcCallMeta = new();
            rpcCallMeta.ArenaPtr = (uint)ptr;
            rpcCallMeta.ArenaLen = (uint)len;
            rpcCallMeta.TargetClientId = hasSpecifiedTarget ? specifiedTargetClient : _selfClientId;
            rpcCallMeta.ObjectId = identity.ObjectId;
            rpcCallMeta.MethodId = _hashToIdx[methodName];
            rpcCallMeta.SubObjectId = identity.SubObjectId;
            rpcCallMeta.Type = (byte)targets;

            _currentCallBuffer.Add(rpcCallMeta);
        }

        private void OnObjectRemove(PositronNetworkIdentity identity)
        {
            if (_dellayedCalls.TryGetValue(identity, out DellayedCall call))
            {
                PositronNetworkWriter w = call.Writer;
                PositronFacade.NetworkIoPool.PutWriter(w);

                _dellayedCalls.Remove(identity);
            }
        }

        private void OnLocalTargetInitedSuccesfully(PositronNetworkIdentity creationId)
        {
            if (_dellayedCalls.TryGetValue(creationId, out DellayedCall call))
            {
                _dellayedCalls.Remove(creationId);
                SendRpcToServer(call.Obj, call.BindedGameObject, call.MethodName, call.SpecifiedTargetClient, call.HasSpecifiedTarget, call.Targets, call.Writer);
            }
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

            if (identity == null)
            {
                PositronFacade.NetworkIoPool.PutReader(reader);
                return;
            }

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

        private struct DellayedCall
        {
            public IRpcTarget Obj;
            public GameObject BindedGameObject;
            public ulong MethodName;
            public uint SpecifiedTargetClient;
            public bool HasSpecifiedTarget;
            public RpcTargets Targets;
            public PositronNetworkWriter Writer;

            public DellayedCall(IRpcTarget obj, GameObject bindedGameObject, ulong methodName, uint specifiedTargetClient, bool hasSpecifiedTarget, RpcTargets targets, PositronNetworkWriter writer)
            {
                Obj = obj;
                BindedGameObject = bindedGameObject;
                MethodName = methodName;
                SpecifiedTargetClient = specifiedTargetClient;
                HasSpecifiedTarget = hasSpecifiedTarget;
                Targets = targets;
                Writer = writer;
            }
        }
    }
}
using Positron.BytesArena;
using Positron.Client.GameEntities;
using Positron.Client.Interfaces;
using Positron.Client.Mono;
using Positron.Client.NetValues;
using Positron.Client.Room.Models.Interfaces;
using Positron.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkValuesModel : IDisposable
    {
        private readonly PooledDynamicArraySegment<NetValue> _currentAddDelta = new(128);
        private readonly PooledDynamicArraySegment<PersistentNetValue> _currentModDelta = new(128);
        private readonly IPositronSerializer _positronSerializer;

        private readonly byte[] _temporarySerializeBuffer = new byte[128 * 1024];
        private readonly TransientArena _allDeltasRawDataArena = new();
        private readonly TransientArena _incomingArena = new();

        private readonly Dictionary<uint, INetValueManaged> _valueIdToValuesInterfaceMapping = new();

        private IReadOnlyGameObjectsModel _gameObjectsModel;

        public NetworkValuesModel(IPositronSerializer serializer)
        {
            _positronSerializer = serializer;
        }

        public void Init(IReadOnlyGameObjectsModel gameObjectsModel)
        {
            TryUnsubGameObjectCallbacks();

            _gameObjectsModel = gameObjectsModel;
            _gameObjectsModel.localObjectRemotelyInitedSuccessfully += OnLocalTargetInitedSuccesfully;
        }

        public void Dispose()
        {
            ClearWorld();

            _currentAddDelta.Dispose();
            _currentModDelta.Dispose();

            TryUnsubGameObjectCallbacks();
        }

        public void ClearWorld()
        {
            ClearDelta();
            _valueIdToValuesInterfaceMapping.Clear();

            // now all values binded to GO`s and mono, it will destroyed automatically
        }

        public void PutArenaFromServer(ReadOnlyMemory<byte> data) => _incomingArena.CloneFrom(data);

        public void PerformAddition(ArraySegment<NetValue> values, bool isLateJoin)
        {
            foreach (NetValue value in values)
            {
                AddValue(value, isLateJoin);
            }
        }

        public void PerformModification(ArraySegment<PersistentNetValue> values)
        {
            foreach (PersistentNetValue value in values)
            {
                ModValue(value);
            }
        }

        private void TryUnsubGameObjectCallbacks()
        {
            if (_gameObjectsModel == null)
            {
                return;
            }

            _gameObjectsModel.localObjectRemotelyInitedSuccessfully -= OnLocalTargetInitedSuccesfully;
        }

        private void AddValue(NetValue value, bool isLateJoin)
        {
            PositronNetworkIdentity identity = _gameObjectsModel.GetObjectByIds(value.ParentObjectId, 0);

            if (identity == null)
            {
                Debug.LogError($"Critical internal error -> identity by id {value.ParentObjectId} is null locally on this machine !");
                return;
            }

            INetValueManaged[] netValues = identity.GetAllNetValues();

            if (value.ValueId >= netValues.Length)
            {
                Debug.LogError($"Critical internal error -> unexpexted value id {value.ValueId} on object {identity}", identity);
                return;
            }

            INetValueManaged managedValue = netValues[value.ValueId];

            if (!managedValue.IsFullyInited)
            {
                if (_valueIdToValuesInterfaceMapping.ContainsKey(value.FlatArrayIdDescriptor))
                {
                    Debug.LogError($"Critical internal error -> doubling of attemps to init value {value.FlatArrayIdDescriptor}");
                    return;
                }

                _valueIdToValuesInterfaceMapping.Add(value.FlatArrayIdDescriptor, managedValue);
                managedValue.MarkInited(value.FlatArrayIdDescriptor);

                managedValue.dataChangedWithFullCallback += OnValueDataChanged;
            }

            if (identity.IsMine && !isLateJoin)
            {
                return;
            }

            Memory<byte> payload = _incomingArena.ReadAsMem(value.ArenaPtr, value.ArenaLen);
            managedValue.DeserializeSelfFrom(payload, _positronSerializer);
        }

        private void ModValue(PersistentNetValue value)
        {
            if(!_valueIdToValuesInterfaceMapping.TryGetValue(value.FlatArrayIdDescriptor, out INetValueManaged managedValue))
            {
                return;
            }

            if (!managedValue.IsFullyInited || managedValue == null)
            {
                _valueIdToValuesInterfaceMapping.Remove(value.FlatArrayIdDescriptor);
                Debug.LogError($"internal critical error -> can`t mod value (FAIDD: {value.FlatArrayIdDescriptor}) that is not fully inited or null!");
                return;
            }

            if (value.IsDeleting)
            {
                managedValue.dataChangedWithFullCallback -= OnValueDataChanged;
                _valueIdToValuesInterfaceMapping.Remove(value.FlatArrayIdDescriptor);
                return;
            }

            Memory<byte> payload = _incomingArena.ReadAsMem(value.ArenaPtr, value.ArenaLen);
            managedValue.DeserializeSelfFrom(payload, _positronSerializer);
        }

        private void OnLocalTargetInitedSuccesfully(PositronNetworkIdentity identity) // local addition start point
        {
            if (!identity.IsMine)
            {
                Debug.LogError($"Internal critical error -> object {identity} is not local but attempted to send network values inition", identity);
                return;
            }

            INetValueManaged[] netValues = identity.GetAllNetValues();
            ushort valueId = 0;

            foreach (INetValueManaged netValue in netValues)
            {
                if (valueId == ushort.MaxValue)
                {
                    Debug.LogError($"Internal ciritcal error -> too much values on object {identity}, max amount is {ushort.MaxValue + 1}", identity);
                    break;
                }

                int bytesWritten = netValue.SerializeSelfTo(_temporarySerializeBuffer, _positronSerializer);

                int ptr = _allDeltasRawDataArena.Alloc(_temporarySerializeBuffer.AsSpan(0, bytesWritten), out int arenaLen);

                NetValue netValueStruct = new();
                netValueStruct.ArenaPtr = (uint)ptr;
                netValueStruct.ArenaLen = (uint)arenaLen;
                netValueStruct.ParentObjectId = identity.ObjectId;
                netValueStruct.ValueId = valueId;
                netValueStruct.Deleting = false;

                valueId++;

                _currentAddDelta.Add(netValueStruct);
            }
        }

        private void OnValueDataChanged(INetValueManaged managedValue, uint flatArrayIdDescriptor) // local modification start point
        {
            if (!managedValue.IsFullyInited)
            {
                return;
            }

            int bytesWritten = managedValue.SerializeSelfTo(_temporarySerializeBuffer, _positronSerializer);
            int ptr = _allDeltasRawDataArena.Alloc(_temporarySerializeBuffer.AsSpan(0, bytesWritten), out int arenaLen);

            PersistentNetValue persistentNetValue = new();
            persistentNetValue.ArenaPtr = (uint)ptr;
            persistentNetValue.ArenaLen = (uint)arenaLen;
            persistentNetValue.FlatArrayIdDescriptor = flatArrayIdDescriptor;
            persistentNetValue.IsDeleting = false;  

            _currentModDelta.Add(persistentNetValue);   
        }

        public ArraySegment<NetValue> GetValuesAddDelta() => _currentAddDelta.ToArray();
        public ArraySegment<PersistentNetValue> GetValuesModDelta() => _currentModDelta.ToArray();
        public ReadOnlySpan<byte> GetArena() => _allDeltasRawDataArena.Data;
       
        public void ClearDelta()
        {
            _currentAddDelta.Clear();
            _currentModDelta.Clear();
            _allDeltasRawDataArena.Flush();
        }
    }
}
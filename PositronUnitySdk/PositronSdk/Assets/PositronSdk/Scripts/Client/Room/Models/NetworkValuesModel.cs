using Positron.BytesArena;
using Positron.Client.GameEntities;
using Positron.Client.Mono;
using Positron.Client.Room.Models.Interfaces;
using Positron.Utility;
using System;
using System.Collections.Generic;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkValuesModel : IDisposable
    {
        private readonly PooledDynamicArraySegment<NetValue> _currentAddDelta = new(128);
        private readonly PooledDynamicArraySegment<PersistentNetValue> _currentModDelta = new(128);
        private readonly TransientArena _allDeltasRawDataArena = new();

        private readonly Dictionary<uint, PositronNetworkIdentity> _worldMapping = new();

        private IReadOnlyGameObjectsModel _gameObjectsModel;

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
            _worldMapping.Clear();

            // now all values binded to GO`s and mono, it will destroyed automatically
        }

        public void PutArenaFromServer(ReadOnlyMemory<byte> data) => _allDeltasRawDataArena.CloneFrom(data);

        public void PerformAddition(ArraySegment<NetValue> values)
        {

        }

        public void PerformModification(ArraySegment<PersistentNetValue> values)
        {

        }

        private void TryUnsubGameObjectCallbacks()
        {
            if (_gameObjectsModel == null)
            {
                return;
            }

            _gameObjectsModel.localObjectRemotelyInitedSuccessfully -= OnLocalTargetInitedSuccesfully;
        }


        private void OnLocalTargetInitedSuccesfully(PositronNetworkIdentity identity)
        {
            // send event to server, collect all values from object
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
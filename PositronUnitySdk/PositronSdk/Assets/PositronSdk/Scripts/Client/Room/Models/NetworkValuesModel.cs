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

        private readonly Dictionary<uint, PositronNetworkIdentity> _valueIdToValuesInterfaceMapping = new();

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
            _valueIdToValuesInterfaceMapping.Clear();

            // now all values binded to GO`s and mono, it will destroyed automatically
        }

        public void PutArenaFromServer(ReadOnlyMemory<byte> data) => _allDeltasRawDataArena.CloneFrom(data);

        public void PerformAddition(ArraySegment<NetValue> values) //this method only SERVER -> CLIENT
        {
            //find network object and value slot
            //init value
            //put data into it
            //put to dict mapping by flat id direcly by interface
            //if it is deleting - clear value slot and mark values is invalid. delete from mapping
        }

        public void PerformModification(ArraySegment<PersistentNetValue> values) //this method only SERVER -> CLIENT
        {
            //find in mapping
            //validate for existance
            //put data
        }

        private void TryUnsubGameObjectCallbacks()
        {
            if (_gameObjectsModel == null)
            {
                return;
            }

            _gameObjectsModel.localObjectRemotelyInitedSuccessfully -= OnLocalTargetInitedSuccesfully;
        }


        private void OnLocalTargetInitedSuccesfully(PositronNetworkIdentity identity) //this method only CLIENT -> SERVER
        {
            //get all values from object (carriers must be alligned in editor time deterministically and cached for 100% determinism)
                //get all INetValueCarrier
                //get all values from carriers
            //serialize data
            //put serialized data into arena
            //construct NetValue structures
            //put structures into addition delta
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
using System.Collections.Generic;
using Positron.Client.GameEntities;
using System;
using Positron.Client.Settings;
using UnityEngine;
using Positron.Client.Mono;
using Positron.Client.GameEntities.Premitive;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkGameObjectsModel : IDisposable
    {
        private readonly NetworkWorld _world;

        private readonly List<NetGameObject> _creationDelta = new(128);
        private readonly List<NetTransform> _moveDelta = new(128);
        private readonly List<uint> _destroyDelta = new(128);

        private readonly Dictionary<PositronNetworkIdentity, ulong> _indexedAssets = new();
        private readonly Dictionary<ulong, PositronNetworkIdentity> _reverseAssetsIndex = new();
        private readonly Dictionary<ulong, PositronNetworkIdentity> _localCreationMapping = new();

        private readonly Dictionary<uint, PositronNetworkIdentity> _currentGameObjectsOnScene = new();

        private ulong _lastCrationId;

        public NetworkGameObjectsModel(NetworkWorld world, PositronSettings settings)
        {
            _world = world;

            for (uint i = 0; i < settings.SpawnableObjects.Length; i++)
            {
                _indexedAssets.Add(settings.SpawnableObjects[i], i);
                _reverseAssetsIndex.Add(i, settings.SpawnableObjects[i]);
            }

            _lastCrationId = 0;
        }

        public void Dispose()
        {
            ClearWorld();
        }

        public void ClearWorld()
        {

        }

        public void CreateLocalObjectAndSendToServer(PositronNetworkIdentity prefab, Vector3 position, Quaternion rotation)
        {
            if (!_indexedAssets.TryGetValue(prefab, out ulong assetIndex))
            {
                Debug.LogError("Critical positron error -> unable to create network object while it is no registred in settings!!!", prefab);
                return;
            }

            _lastCrationId++;

            NetGameObject networkObject = new();
            networkObject.AssetIndex = assetIndex;
            networkObject.CreationId = _lastCrationId;
            networkObject.ObjectId = 0;
            networkObject.OwnerClientId = _world.LocalClientId;
            networkObject.Position = new NetVector3(position.x, position.y, position.z);
            networkObject.Rotation = new NetVector3(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);

            PositronNetworkIdentity created = GameObject.Instantiate(prefab, position, rotation);
            created.LocalInit(networkObject.CreationId, networkObject.OwnerClientId);

            _creationDelta.Add(networkObject);
            _localCreationMapping.Add(networkObject.CreationId, created);
        }

        public void CreateObjects(NetGameObject[] objs)
        {
            foreach (NetGameObject obj in objs)
            {
                SpawnObject(obj);
            }
        }

        public void SpawnObject(NetGameObject obj)
        {
            if (obj.OwnerClientId == _world.LocalClientId && _localCreationMapping.TryGetValue(obj.CreationId, out PositronNetworkIdentity localCopy))
            {
                localCopy.NetworkInit(obj);
            }
            else
            {
                // spawn from scratch
            }
        }

        public void RemoveObjects(uint[] objs)
        {
            foreach (uint obj in objs)
            {
                DestyroyObject(obj);
            }
        }

        public void DestyroyObject(uint obj)
        {

        }

        public void TransferedObjects(uint[] objs, uint actualHost)
        {

        }

        public void MoveObjects(NetTransform[] objs)
        {

        }

        public GameObjectsDelta GetActionsDelta() => new GameObjectsDelta(_creationDelta.ToArray(), _destroyDelta.ToArray());
        public NetTransform[] GetMoveDelta() => _moveDelta.ToArray();
        public void ClearDelta()
        {
            _creationDelta.Clear();
            _destroyDelta.Clear();
            _moveDelta.Clear();
        }

        public struct GameObjectsDelta
        {
            public NetGameObject[] NewGameOgjects;
            public uint[] RemovedGameObjectIds;
            
            public GameObjectsDelta(NetGameObject[] gos, uint[] destruction)
            {
                NewGameOgjects = gos;
                RemovedGameObjectIds = destruction;
            }
        }
    }
}
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
        private readonly List<ulong> _localCreationBlacklist = new();
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
            foreach (KeyValuePair<uint, PositronNetworkIdentity> obj in _currentGameObjectsOnScene)
            {
                GameObject.Destroy(obj.Value);
            }

            foreach (KeyValuePair<ulong, PositronNetworkIdentity> obj in _localCreationMapping)
            {
                GameObject.Destroy(obj.Value);
            }
            
            _localCreationMapping.Clear();  
            _localCreationBlacklist.Clear();
            _currentGameObjectsOnScene.Clear();

            ClearDelta();
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

        public void DeleteObjectAndSendToServer(PositronNetworkIdentity instance)
        {
            if (!_localCreationMapping.ContainsKey(instance.CreationId) && !_currentGameObjectsOnScene.ContainsKey(instance.ObjectId))
            {
                Debug.LogError($"Positron error -> object instance {instance.gameObject} is not found in current approved network objects or network objects local mapping!");
                return;
            }

            if (_localCreationMapping.ContainsKey(instance.CreationId))
            {
                int findIndex = -1;

                for (int i = 0; i < _creationDelta.Count; i++)
                {
                    if (_creationDelta[i].CreationId == instance.CreationId)
                    {
                        findIndex = i;
                        break;
                    }
                }

                if (findIndex == -1)
                {
                    _localCreationBlacklist.Add(instance.CreationId);
                }
                else
                {
                    _creationDelta.RemoveAt(findIndex);
                    _localCreationMapping.Remove(instance.CreationId);
                }

                GameObject.Destroy(instance);
                return;
            }

            _destroyDelta.Add(instance.ObjectId);
            GameObject.Destroy(instance);
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
                _localCreationMapping.Remove(obj.CreationId);

                if (_localCreationBlacklist.Contains(obj.CreationId))
                {
                    _destroyDelta.Add(obj.ObjectId);
                    _localCreationBlacklist.Remove(obj.CreationId);
                    return;
                }

                if (localCopy == null)
                {
                    Debug.LogError("Positron unexpected error -> local copy is null!");
                    return;
                }

                localCopy.NetworkInit(obj);

                _currentGameObjectsOnScene.Add(obj.ObjectId, localCopy);
            }
            else
            {
                if (!_reverseAssetsIndex.ContainsKey(obj.AssetIndex))
                {
                    Debug.LogError($"Critical error -> received from network creation of obj with asset index '{obj.AssetIndex}' that not exists!!! Check version missmatch");
                    return;
                }

                PositronNetworkIdentity created = GameObject.Instantiate(_reverseAssetsIndex[obj.AssetIndex], obj.Position.ToUnity(), Quaternion.Euler(obj.Rotation.ToUnity()));
                created.NetworkInit(obj);

                _currentGameObjectsOnScene.Add(obj.ObjectId, created);
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
            if (_currentGameObjectsOnScene.ContainsKey(obj))
            {
                GameObject sceneObj = _currentGameObjectsOnScene[obj].gameObject;
                _currentGameObjectsOnScene.Remove(obj);
                GameObject.Destroy(sceneObj);
            }
        }

        public void TransferedObjects(uint[] objs, uint actualHost)
        {
            foreach(uint objId in objs)
            {
                if (_currentGameObjectsOnScene.TryGetValue(objId, out PositronNetworkIdentity instance))
                {
                    instance.Transfer(actualHost);
                }
            }
        }

        public void CollectCurrentObjectsMoveDeltas()
        {
            foreach (KeyValuePair<uint, PositronNetworkIdentity> networkObjectPair in _currentGameObjectsOnScene)
            {
                if (networkObjectPair.Value.CheckForMoved())
                {
                    NetTransform deltaData = new();
                    deltaData.ObjectId = networkObjectPair.Key;
                    deltaData.Position = new(networkObjectPair.Value.transform.position);
                    deltaData.Rotation = new(networkObjectPair.Value.transform.eulerAngles);

                    _moveDelta.Add(deltaData);

                    networkObjectPair.Value.RecordPreviousTransform();
                }
            }
        }

        public void MoveObjects(NetTransform[] objs)
        {
            foreach (NetTransform transform in objs)
            {
                if (_currentGameObjectsOnScene.TryGetValue(transform.ObjectId, out PositronNetworkIdentity networkObject))
                {
                    networkObject.SetTransform(transform);
                }
            }
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
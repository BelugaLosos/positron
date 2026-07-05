using System.Collections.Generic;
using Positron.Client.GameEntities;
using System;
using Positron.Client.Settings;
using UnityEngine;
using Positron.Client.Mono;
using Positron.Client.GameEntities.Premitive;
using Positron.Client.Mono.Syncers;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkGameObjectsModel : IDisposable
    {
        private readonly NetworkWorld _world;

        private readonly List<NetGameObject> _creationDelta = new(128);
        private readonly List<NetTransform> _moveDelta = new(128);
        private readonly List<uint> _destroyDelta = new(128);
        private readonly List<uint> _requestOwnershipDelta = new(16);

        private readonly Dictionary<PositronNetworkIdentity, ushort> _indexedAssets = new();
        private readonly Dictionary<ushort, PositronNetworkIdentity> _reverseAssetsIndex = new();

        private readonly Dictionary<ushort, PositronNetworkIdentity> _localCreationMapping = new();
        private readonly List<ushort> _localCreationBlacklist = new();
        private readonly Dictionary<uint, PositronNetworkIdentity> _currentGameObjectsOnScene = new();

        private ushort _lastCrationId;
        private uint _recentTick;

        public NetworkGameObjectsModel(NetworkWorld world, PositronSettings settings)
        {
            _world = world;

            for (ushort i = 0; i < settings.SpawnableObjects.Length; i++)
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
                if (obj.Value == null || obj.Value.gameObject == null)
                {
                    Debug.LogError($"Unexpected not critical error -> obj with id: {obj.Key} is null (Remote replicated instance)");
                    continue;
                }

                GameObject.Destroy(obj.Value.gameObject);
            }

            foreach (KeyValuePair<ushort, PositronNetworkIdentity> obj in _localCreationMapping)
            {
                if (obj.Value == null || obj.Value.gameObject == null)
                {
                    Debug.LogError($"Unexpected not critical error -> obj with id: {obj.Key} is null (Local instance)");
                    continue;
                }

                GameObject.Destroy(obj.Value.gameObject);
            }
            
            _localCreationMapping.Clear();  
            _localCreationBlacklist.Clear();
            _currentGameObjectsOnScene.Clear();

            ClearDelta();
        }

        public void CreateLocalObjectAndSendToServer(PositronNetworkIdentity prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException("Positron error -> can`t spawn null prefab");
            }

            if (!_indexedAssets.TryGetValue(prefab, out ushort assetIndex))
            {
                Debug.LogError("This message is additional info for exception below!", prefab);

                throw new ArgumentException("Critical positron error -> unable to create network object while it is no registred in settings!!!");
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
            if(instance == null)
            {
                throw new ArgumentNullException("Positron error -> can`t destroy null object");
            }

            if (instance.OwnerClientId != _world.LocalClientId)
            {
                throw new ArgumentException($"Positron critical error -> can`t destroy not local owned object OBJ: '{instance}' OWNED_BY: '{instance.OwnerClientId}' LOCAL_ID: '{_world.LocalClientId}'");
            }

            if (!_localCreationMapping.ContainsKey(instance.CreationId) && !_currentGameObjectsOnScene.ContainsKey(instance.ObjectId))
            {
                throw new ArgumentException($"Positron error -> object instance {instance.gameObject} is not found in current approved network objects or network objects local mapping!");
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

                GameObject.Destroy(instance.gameObject);
                return;
            }

            _destroyDelta.Add(instance.ObjectId);
            GameObject.Destroy(instance.gameObject);
        }

        public void RequestOwnership(PositronNetworkIdentity networkIdentity)
        {
            if (!networkIdentity.IsFullyInitialized)
            {
                return;
            }

            if (networkIdentity.IsFullyInitialized && networkIdentity.IsMine)
            {
                return;
            }

            if (networkIdentity.SubObjectId != 0)
            {
                throw new ArgumentException("Critical error -> can`t request ownership on sub object, must be requested on parent !!!");
            }

            _requestOwnershipDelta.Add(networkIdentity.ObjectId);
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
            if (_currentGameObjectsOnScene.TryGetValue(obj, out PositronNetworkIdentity localInstance))
            {
                _currentGameObjectsOnScene.Remove(obj);

                if (localInstance == null)
                {
                    return;
                }

                GameObject sceneObj = localInstance.gameObject;
                GameObject.Destroy(sceneObj);
            }
        }

        public void TransferedObjects(uint[] objs, uint actualHost)
        {
            foreach(uint objId in objs)
            {
                if (_currentGameObjectsOnScene.TryGetValue(objId, out PositronNetworkIdentity instance))
                {
                    if (instance == null)
                    {
                        continue;
                    }

                    instance.Transfer(actualHost);
                }
            }
        }

        public void PerformTargetatedTransfer(uint[] transferedDataBuffer)
        {
            for (int i = 0; i < transferedDataBuffer.Length - 1; i += 2)
            {
                uint newOwnerId = transferedDataBuffer[i];
                uint oid = transferedDataBuffer[i + 1];

                if (_currentGameObjectsOnScene.TryGetValue(oid, out PositronNetworkIdentity obj))
                {
                    obj.Transfer(newOwnerId);
                }
            }
        }

        public void CollectCurrentObjectsMoveDeltas()
        {
            foreach (KeyValuePair<uint, PositronNetworkIdentity> networkObjectPair in _currentGameObjectsOnScene)
            {
                if (networkObjectPair.Value == null)
                {
                    continue;
                }

                if (networkObjectPair.Value.TryGetSyncer(out PositronTransformSync transformSyncer) && 
                    transformSyncer.CheckForMoved())
                {
                    NetTransform deltaData = new();
                    deltaData.ObjectId = networkObjectPair.Key;
                    deltaData.Position = new(networkObjectPair.Value.transform.position);
                    deltaData.Rotation = new(networkObjectPair.Value.transform.eulerAngles);

                    _moveDelta.Add(deltaData);

                    transformSyncer.RecordPreviousTransform();
                }
            }
        }

        public void MoveObjects(NetTransform[] objs, uint tickIndex)
        {
            if (tickIndex < _recentTick)
            {
                return;
            }

            _recentTick = tickIndex;

            foreach (NetTransform transform in objs)
            {
                if (_currentGameObjectsOnScene.TryGetValue(transform.ObjectId, out PositronNetworkIdentity networkObject))
                {
                    bool hasTransformSyncer = networkObject.TryGetSyncer(out PositronTransformSync syncer);

                    if (networkObject == null || !hasTransformSyncer || networkObject.IsMine)
                    {
                        continue;
                    }

                    syncer.SetTransform(transform, tickIndex);
                }
            }
        }

        public GameObjectsDelta GetActionsDelta() => new GameObjectsDelta(_creationDelta.ToArray(), _destroyDelta.ToArray(), _requestOwnershipDelta.ToArray());
        public NetTransform[] GetMoveDelta() => _moveDelta.ToArray();

        public void ClearDelta()
        {
            _creationDelta.Clear();
            _destroyDelta.Clear();
            _moveDelta.Clear();
            _requestOwnershipDelta.Clear();
        }

        public struct GameObjectsDelta
        {
            public NetGameObject[] NewGameOgjects;
            public uint[] RemovedGameObjectIds;
            public uint[] RequestOwnershipDelta;
            
            public GameObjectsDelta(NetGameObject[] gos, uint[] destruction, uint[] requestOwnershipDelta)
            {
                NewGameOgjects = gos;
                RemovedGameObjectIds = destruction;
                RequestOwnershipDelta = requestOwnershipDelta;
            }
        }
    }
}
using Positron.Client.GameEntities;
using Positron.Client.Mono.Interfaces;
using Positron.Client.Mono.Syncers.Interface;
using Positron.Client.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Positron.Client.Mono
{
    public class PositronNetworkIdentity : MonoBehaviour
    {
        [SerializeField] private PositronNetworkIdentity[] _trackedSubObjects;

        private IPositronSyncer[] _syncers;
        private IRpcTarget[] _observedRpcTargets;
        private Dictionary<Type, IPositronSyncer> _syncersMap = new();

        private bool _isLocallyInited;
        private bool _activityStateInited;

        public ushort CreationId { get; private set; }
        public uint ObjectId { get; private set; }
        public ushort SubObjectId { get; private set; }
        public uint OwnerClientId { get; private set; }
        public bool IsFullyInitialized { get; private set; }
        public bool OriginalActivityState { get; private set; }
        public bool IsObjectFullyAvailable { get; private set; }  

        public bool IsMine => PositronFacade.World.LocalClientId == OwnerClientId || !PositronFacade.World.InRoom;
        public bool IsHost => PositronFacade.World.HostId == OwnerClientId || !PositronFacade.World.InRoom;

        public event Action<PositronNetworkIdentity> completeInitialize;
        public event Action completeInitWithEmptyCallback;
        public event Action<PositronNetworkIdentity> transfered;
        public event Action transferedWithEmptyCallback;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            _trackedSubObjects = GetComponentsInChildren<PositronNetworkIdentity>().Where(o => o != this).ToArray();
        }
#endif

        public void InitActivityState(bool activity)
        {
            if (_activityStateInited)
            {
                return;
            }

            OriginalActivityState = activity;
            _activityStateInited = true;
        }

        public void SetObjectFullyAvailable()
        {
            IsObjectFullyAvailable = true;

            foreach (PositronNetworkIdentity obj in _trackedSubObjects)
            {   
                obj.IsObjectFullyAvailable = true;
            }

            foreach (INetworkAwakeble awakeble in GetComponentsInChildren<INetworkAwakeble>())
            {
                awakeble.OnNetworkAwake(); 
            }
        }

        public void SendDestroyCallbacks()
        {
            foreach (INetworkDestructable destructable in GetComponentsInChildren<INetworkDestructable>())
            {
                destructable.OnNetworkDestroy();
            }
        }

        public void LocalInit(ushort creationId, uint owner)
        {
            if (_isLocallyInited)
            {
                Debug.LogError("Positron error -> can`t do local init twice");
                return;
            }

            InitLocally(creationId, owner);

            _syncers = GetComponents<IPositronSyncer>();

            InitTrackedSubObjects(true, creationId, owner, default);
            InitSyncers();
        }

        public void NetworkInit(NetGameObject networkData)
        {
            if (IsFullyInitialized)
            {
                Debug.LogError("Positron error -> can`t do network init twice");
                return;
            }

            InitGlobally(networkData);

            _syncers = GetComponents<IPositronSyncer>();
            
            InitTrackedSubObjects(false, 0, 0, networkData);
            InitSyncers();

            completeInitialize?.Invoke(this);
            completeInitWithEmptyCallback?.Invoke();
        }

        public void Transfer(uint actualOwner)
        {
            if (!IsFullyInitialized)
            {
                Debug.LogError($"Positron critical error -> unable to transfer object that not inited yet {gameObject.name}", gameObject);
                return;
            }

            OwnerClientId = actualOwner;

            transfered?.Invoke(this);
            transferedWithEmptyCallback?.Invoke();
        }

        public bool TryGetSyncer<TSyncer>(out TSyncer syncer) where TSyncer : IPositronSyncer
        {
            if (_syncersMap.TryGetValue(typeof(TSyncer), out IPositronSyncer getten))
            {
                syncer = (TSyncer)getten;
                return true;
            }

            syncer = default;
            return false;
        }

        public PositronNetworkIdentity GetSubObject(ushort subObjectId)
        {
            int index = subObjectId - 1;
            return _trackedSubObjects[index];
        }

        public IRpcTarget[] GetObservedRpcTargets()
        {
            if (_observedRpcTargets == null)
            {
                _observedRpcTargets = GetComponentsInChildren<IRpcTarget>();
            }

            return _observedRpcTargets;
        }

        private void InitLocally(ushort creationId, uint owner)
        {
            CreationId = creationId;
            OwnerClientId = owner;

            _isLocallyInited = true;
        }

        private void InitGlobally(NetGameObject networkData)
        {
            CreationId = 0;
            ObjectId = networkData.ObjectId;
            OwnerClientId = networkData.OwnerClientId;

            IsFullyInitialized = true;
            _isLocallyInited = true;
        }

        private void InitSyncers()
        {
            _syncersMap.Clear();

            foreach (IPositronSyncer syncer in _syncers)
            {
                syncer.Init(this);
                _syncersMap.Add(syncer.GetType(), syncer);
            }
        }

        private void InitTrackedSubObjects(bool isLocalInit, ushort creationId, uint owner, NetGameObject networkData)
        {
            for (int i = 0; i < _trackedSubObjects.Length; i++)
            {
                if (_trackedSubObjects[i] == null)
                {
                    continue;
                }

                _trackedSubObjects[i].SubObjectId = (ushort)((ushort)i + 1);

                if (isLocalInit)
                {
                    _trackedSubObjects[i].InitLocally(creationId, owner);
                }
                else
                {
                    _trackedSubObjects[i].InitGlobally(networkData);
                }
            }
        }
    }
}
using Positron.Client.GameEntities;
using Positron.Client.Mono.Syncers.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Positron.Client.Mono
{
    public class PositronNetworkIdentity : MonoBehaviour
    {
        [SerializeField] private PositronNetworkIdentity[] _trackedSubObjects;

        private IPositronSyncer[] _syncers;
        private Dictionary<Type, IPositronSyncer> _syncersMap = new();

        private bool _isLocallyInited;

        public ushort CreationId { get; private set; }
        public uint ObjectId { get; private set; }
        public ushort SubObjectId { get; private set; }
        public uint OwnerClientId { get; private set; }
        public bool IsFullyInitialized { get; private set; }

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

            _trackedSubObjects = GetComponentsInChildren<PositronNetworkIdentity>();
        }
#endif

        public void LocalInit(ushort creationId, uint owner)
        {
            if (_isLocallyInited)
            {
                Debug.LogError("Positron error -> can`t do local init twice");
                return;
            }

            CreationId = creationId;
            OwnerClientId = owner;

            _isLocallyInited = true;

            _syncers = GetComponents<IPositronSyncer>();

            InitTrackedSubObjects();
            InitSyncers();
        }

        public void NetworkInit(NetGameObject networkData)
        {
            if (IsFullyInitialized)
            {
                Debug.LogError("Positron error -> can`t do network init twice");
                return;
            }

            CreationId = 0;
            ObjectId = networkData.ObjectId;
            OwnerClientId = networkData.OwnerClientId;

            IsFullyInitialized = true;
            _isLocallyInited = true;

            _syncers = GetComponents<IPositronSyncer>();
            
            InitTrackedSubObjects();
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

        private void InitSyncers()
        {
            _syncersMap.Clear();

            foreach (IPositronSyncer syncer in _syncers)
            {
                syncer.Init(this);
                _syncersMap.Add(syncer.GetType(), syncer);
            }
        }

        private void InitTrackedSubObjects()
        {
            for (int i = 0; i < _trackedSubObjects.Length; i++)
            {
                if (_trackedSubObjects[i] == null)
                {
                    continue;
                }

                _trackedSubObjects[i].SubObjectId = (ushort)((ushort)i + 1);
            }
        }
    }
}
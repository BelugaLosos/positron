using Positron.Client.GameEntities;
using System;
using UnityEngine;

namespace Positron.Client.Mono
{
    public class PositronNetworkIdentity : MonoBehaviour
    {
        [SerializeField] private bool _syncTransform = true;

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;

        private bool _isLocallyInited;

        private float _previousSyncFrameTime;
        private float _currentSyncFrameTime;

        private Vector3 _targetPosition;
        private Vector3 _startPosition;

        private Quaternion _targetRotation;
        private Quaternion _startRotation;

        public ulong CreationId { get; private set; }
        public uint ObjectId { get; private set; }
        public ushort SubObjectId { get; private set; }
        public uint OwnerClientId { get; private set; }
        public bool IsFullyInitialized { get; private set; }
        public bool IsNeedSyncTransform => _syncTransform;

        public bool IsMine => PositronFacade.World.LocalClientId == OwnerClientId || !PositronFacade.World.InRoom;
        public bool IsHost => PositronFacade.World.HostId == OwnerClientId || !PositronFacade.World.InRoom;

        public event Action<PositronNetworkIdentity> completeInitialize;
        public event Action<PositronNetworkIdentity> transfered;

        private const float DISTANCE_TO_SYNC = 0.05f;

        private void Update()
        {
            if (IsMine)
            {
                return;
            }

            float estamted = Time.time - _previousSyncFrameTime;
            float duration = _currentSyncFrameTime - _previousSyncFrameTime + 0.0001f;

            float percent = Mathf.Clamp01(estamted / duration);

            transform.position = Vector3.Lerp(_startPosition, _targetPosition, percent);
            transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, percent);
        }

        public void LocalInit(ulong creationId, uint owner)
        {
            if (_isLocallyInited)
            {
                Debug.LogError("Positron error -> can`t do local init twice");
                return;
            }

            CreationId = creationId;
            OwnerClientId = owner;

            InitTransformTargets();

            _isLocallyInited = true;
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

            InitTransformTargets();

            IsFullyInitialized = true;
            _isLocallyInited = true;

            completeInitialize?.Invoke(this);
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
        }

        public void RecordPreviousTransform()
        {
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;
        }

        public bool CheckForMoved() => Vector3.Distance(transform.position, _previousPosition) > DISTANCE_TO_SYNC || transform.rotation != _previousRotation;

        public void SetTransform(NetTransform netTransform)
        {
            _startPosition = transform.position;
            _targetPosition = netTransform.Position.ToUnity();

            _startRotation = transform.rotation;
            _targetRotation = Quaternion.Euler(netTransform.Rotation.ToUnity());

            _previousSyncFrameTime = _currentSyncFrameTime;
            _currentSyncFrameTime = Time.time;
        }

        private void InitTransformTargets()
        {
            _startPosition = transform.position;
            _targetPosition = transform.position;

            _startRotation = transform.rotation;
            _targetRotation = transform.rotation;

            _previousSyncFrameTime = Time.time;
            _currentSyncFrameTime = Time.time;
        }
    }
}
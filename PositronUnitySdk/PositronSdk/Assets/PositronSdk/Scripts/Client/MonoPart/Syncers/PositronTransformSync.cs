using Positron.Client.GameEntities;
using Positron.Client.Mono.Syncers.Interface;
using UnityEngine;

namespace Positron.Client.Mono.Syncers
{
    [RequireComponent(typeof(PositronNetworkIdentity))]
    public class PositronTransformSync : MonoBehaviour, IPositronSyncer
    {
        private PositronNetworkIdentity _identity;

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;

        private float _previousSyncFrameTime;
        private float _currentSyncFrameTime;

        private Vector3 _targetPosition;
        private Vector3 _startPosition;

        private Quaternion _targetRotation;
        private Quaternion _startRotation;

        private const float DISTANCE_TO_SYNC = 0.05f;

        public void Init(PositronNetworkIdentity parent)
        {
            _identity = parent;
            InitTransformTargets();
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

        private void Update()
        {
            if (_identity.IsMine)
            {
                return;
            }

            float estamted = Time.time - _previousSyncFrameTime;
            float duration = _currentSyncFrameTime - _previousSyncFrameTime + 0.0001f;

            float percent = Mathf.Clamp01(estamted / duration);

            transform.position = Vector3.Lerp(_startPosition, _targetPosition, percent);
            transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, percent);
        }
    }
}
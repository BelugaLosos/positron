using Positron.Client.GameEntities;
using Positron.Client.Mono.Syncers.Interface;
using System.Collections.Generic;
using UnityEngine;

namespace Positron.Client.Mono.Syncers
{
    [RequireComponent(typeof(PositronNetworkIdentity))]
    public class PositronTransformSync : MonoBehaviour, IPositronSyncer
    {
        private PositronNetworkIdentity _identity;
        private List<TransformSnapshot> _snapshotsBuffer = new();

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;

        private const float DISTANCE_TO_SYNC = 0.05f;
        private const float ANGLE_TO_SYNC = 1f;

        public void Init(PositronNetworkIdentity parent)
        {
            _identity = parent;
        }

        public void RecordPreviousTransform()
        {
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;
        }

        public bool CheckForMoved() => Vector3.Distance(transform.position, _previousPosition) > DISTANCE_TO_SYNC || Quaternion.Angle(transform.rotation, _previousRotation) > ANGLE_TO_SYNC;
        public void SetTransform(NetTransform netTransform, uint tickIndex) => _snapshotsBuffer.Add(new(tickIndex, netTransform));

        private void Update()
        {
            if (_identity.IsMine)
            {
                return;
            }

            double currentTime = PositronFacade.World.NetworkTime;

            CleanUpBuffer(currentTime);

            if (_snapshotsBuffer.Count >= 2)
            {
                TransformSnapshot left = _snapshotsBuffer[0];
                TransformSnapshot right = _snapshotsBuffer[1];

                if (currentTime >= left.Time && currentTime <= right.Time)
                {
                    Interpolate(left, right, currentTime);
                }
                else if (currentTime > right.Time)
                {
                    DoEmptyBufferFallback(right);
                }
            }
        }

        private void CleanUpBuffer(double currentTime)
        {
            while (_snapshotsBuffer.Count > 2 && _snapshotsBuffer[1].Time < currentTime)
            {
                _snapshotsBuffer.RemoveAt(0);
            }
        }

        private void Interpolate(TransformSnapshot past, TransformSnapshot future, double time)
        {
            double localTime = time - past.Time;
            double length = future.Time - past.Time;
            double elapsedPercent = localTime / length;

            transform.position = Vector3.Lerp(past.Transform.Position.ToUnity(), future.Transform.Position.ToUnity(), (float)elapsedPercent);
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(past.Transform.Rotation.ToUnity()), Quaternion.Euler(future.Transform.Rotation.ToUnity()), (float)elapsedPercent);
        }

        private void DoEmptyBufferFallback(TransformSnapshot right)
        {
            transform.position = right.Transform.Position.ToUnity();
            transform.rotation = Quaternion.Euler(right.Transform.Rotation.ToUnity());
        }

        public struct TransformSnapshot
        {
            public double Time;
            public NetTransform Transform;

            public TransformSnapshot(uint tick, NetTransform transform)
            {
                Time = PositronFacade.World.TickToSeconds(tick);
                Transform = transform;
            }
        }
    }
}
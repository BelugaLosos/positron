using System.Collections.Generic;
using Positron.Client.GameEntities;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkGameObjectsModel : IDisposable
    {
        private readonly NetworkWorld _world;
        private readonly List<NetGameObject> _creationDelta = new(128);
        private readonly List<NetTransform> _moveDelta = new(128);
        private readonly List<uint> _destroyDelta = new(128);

        public NetworkGameObjectsModel(NetworkWorld world)
        {
            _world = world;
        }

        public void Dispose()
        {
            ClearWorld();
        }

        public void ClearWorld()
        {

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
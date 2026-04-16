using Positron.Client.GameEntities;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkGameObjectsModel : IDisposable
    {
        private NetworkWorld _world;

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

        public NetTransform[] GetMoveDiff()
        {
            return default;
        }
    }
}
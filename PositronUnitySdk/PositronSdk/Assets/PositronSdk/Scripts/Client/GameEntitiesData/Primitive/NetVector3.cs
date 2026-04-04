using MessagePack;
using UnityEngine;

namespace Positron.Client.GameEntities.Premitive
{
    [MessagePackObject]
    public struct NetVector3
    {
        [Key(0)] public float[] Cords { get; }

        public NetVector3(float x, float y, float z)
        {
            Cords = new float[3];
            Cords[0] = x;
            Cords[1] = y;
            Cords[2] = z;
        }

        public Vector3 ToUnity() => new(Cords[0], Cords[1], Cords[2]);
    }
}
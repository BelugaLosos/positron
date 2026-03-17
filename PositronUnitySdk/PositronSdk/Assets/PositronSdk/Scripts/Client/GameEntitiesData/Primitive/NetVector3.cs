using MessagePack;
using UnityEngine;

namespace Positron.Client.GameEntities.Premitive
{
    [MessagePackObject]
    public struct NetVector3
    {
        [Key(0)] public float[] Coords { get; }

        public NetVector3(float x, float y, float z)
        {
            Coords = new float[3];
            Coords[0] = x;
            Coords[1] = y;
            Coords[2] = z;
        }

        public Vector3 ToUnity() => new(Coords[0], Coords[1], Coords[2]);
    }
}
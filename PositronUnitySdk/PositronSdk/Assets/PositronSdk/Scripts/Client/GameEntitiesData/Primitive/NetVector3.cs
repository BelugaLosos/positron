using MessagePack;
using UnityEngine;

namespace Positron.Client.GameEntities.Premitive
{
    [MessagePackObject]
    public struct NetVector3
    {
        [Key(0)] public float X { get; }
        [Key(1)] public float Y { get; }
        [Key(2)] public float Z { get; }

        public NetVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToUnity() => new(X, Y, Z);
    }
}
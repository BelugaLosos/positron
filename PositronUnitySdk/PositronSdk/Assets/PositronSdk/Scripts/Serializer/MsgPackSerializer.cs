using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;
using Positron.Client.Interfaces;
using System;

namespace Positron.Serialzier
{
    public sealed class MsgPackSerializer : IPositronSerializer
    {
        public void Init()
        {
            StaticCompositeResolver.Instance.Register(
                GeneratedResolver.Instance,
                BuiltinResolver.Instance,
                AttributeFormatterResolver.Instance,
                PrimitiveObjectResolver.Instance,
                StandardResolver.Instance
            );

            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.
                WithResolver(UnityResolver.InstanceWithStandardResolver);
        }

        public T Deserialize<T>(Span<byte> data)
        {
            return MessagePackSerializer.Deserialize<T>(data.ToArray());
        }

        public Span<byte> Serialize<T>(T data)
        {
            return MessagePackSerializer.Serialize(data);
        }
    }
}
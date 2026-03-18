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
            IFormatterResolver unityResolver = CompositeResolver.Create(
                GeneratedResolver.Instance,          
                AttributeFormatterResolver.Instance, 
                UnityResolver.Instance, 
                StandardResolver.Instance,           
                BuiltinResolver.Instance,            
                PrimitiveObjectResolver.Instance     
            );

            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(unityResolver);
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
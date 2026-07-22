using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;
using Positron.Client.Interfaces;
using System;
using System.IO;

namespace Positron.Serialzier
{
    public sealed class MsgPackSerializer : IPositronSerializer
    {
        private Stream _serializeStream;

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

            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
                .WithResolver(unityResolver)
                .WithCompression(MessagePackCompression.None);

            _serializeStream = new MemoryStream(256 * 1024); // 256 KB
        }

        public int Serialize<T>(T data, Span<byte> destination)
        {
            _serializeStream.SetLength(0);
            MessagePackSerializer.Serialize(_serializeStream, data);

            int length = (int)_serializeStream.Length;

            if (length > destination.Length)
            {
                throw new ArgumentException($"Buffer too small! Required: {length}, Available: {destination.Length}");
            }

            _serializeStream.Position = 0;
            _serializeStream.Read(destination.Slice(0, length));

            return length;
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> data)
        {
            return MessagePackSerializer.Deserialize<T>(data);
        }
    }
}
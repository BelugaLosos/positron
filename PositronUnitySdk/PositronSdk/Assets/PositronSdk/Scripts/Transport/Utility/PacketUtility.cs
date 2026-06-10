using Positron.Client.ConstantHolders;
using System;
using System.Buffers.Binary;

namespace Positron.Transport.Utility
{
    public static class PacketUtility
    {
        public static readonly int PROTOCOL_HEADER_MAX_OFFSET = 6;

        public static Span<byte> GlueDataToOptions(EventTypes eventType, bool isCompressed, uint sourceSize, Span<byte> data, byte[] sharedBuffer)
        {
            byte compressionFlag = 0;
            uint totalLen = (uint)data.Length + 2;

            if (isCompressed)
            {
                compressionFlag = 1;
                totalLen += 4;
            }

            Span<byte> buffer = new Span<byte>(sharedBuffer).Slice(0, (int)totalLen);
            buffer[0] = (byte)eventType;
            buffer[1] = compressionFlag;

            if (isCompressed)
            {
                BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(2, 4), sourceSize);
                data.CopyTo(buffer.Slice(6));
            }
            else
            {
                data.CopyTo(buffer.Slice(2));
            }

            return buffer.Slice(0, (int)totalLen);
        }

        public static PacketData DeconstructPacket(Span<byte> packet)
        {
            PacketData packetWithHeaders = new PacketData();
            packetWithHeaders.Event = (EventTypes)packet[0];
            packetWithHeaders.IsCompressed = packet[1] == 1;
            packetWithHeaders.SourceSize = 0;

            Span<byte> data;

            if (packetWithHeaders.IsCompressed)
            {
                data = packet.Slice(6);
                packetWithHeaders.SourceSize = BinaryPrimitives.ReadUInt32BigEndian(packet.Slice(2, 4));
            }
            else
            {
                data = packet.Slice(2);
                packetWithHeaders.SourceSize = (uint)data.Length;
            }

            packetWithHeaders.Data = data;

            return packetWithHeaders;
        }
    }

    public ref struct PacketData
    {
        public EventTypes Event;
        public bool IsCompressed;
        public uint SourceSize;
        public Span<byte> Data;
    }
}
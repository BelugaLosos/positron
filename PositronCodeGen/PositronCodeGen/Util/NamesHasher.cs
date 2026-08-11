using System.Security.Cryptography;
using System.Text;

namespace PositronCodeGen.Util
{
    internal class NamesHasher
    {
        public ulong To64BitHash(string name)
        {
            SHA256Managed hasher = new SHA256Managed();
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            ulong hash = ReadUInt64BigEndian(hasher.ComputeHash(nameBytes));
            hasher.Dispose();
            return hash;
        }

        private ulong ReadUInt64BigEndian(byte[] data) =>   ((ulong)data[0] << 56) |
                                                            ((ulong)data[1] << 48) |
                                                            ((ulong)data[2] << 40) |
                                                            ((ulong)data[3] << 32) |
                                                            ((ulong)data[4] << 24) |
                                                            ((ulong)data[5] << 16) |
                                                            ((ulong)data[6] << 8)  |
                                                            data[7];
    }
}

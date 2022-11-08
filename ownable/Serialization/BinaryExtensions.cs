using System.Numerics;

namespace ownable.Serialization
{
    public static class BinaryExtensions
    {
        public static void Write(this BinaryWriter bw, Guid value)
        {
            bw.Write(value.ToByteArray());
        }

        public static Guid ReadGuid(this BinaryReader br)
        {
            return new Guid(br.ReadBytes(16));
        }

        public static bool WriteBoolean(this BinaryWriter bw, bool value)
        {
            bw.Write(value);
            return value;
        }

        public static void WriteNullableString(this BinaryWriter bw, string? value)
        {
            if (bw.WriteBoolean(value != null))
                bw.Write(value!);
        }

        public static string? ReadNullableString(this BinaryReader br)
        {
            return br.ReadBoolean() ? br.ReadString() : null;
        }

        public static void Write(this BinaryWriter bw, BigInteger value)
        {
            var buffer = value.ToByteArray();
            bw.Write(buffer.Length);
            bw.Write(buffer);
        }

        public static BigInteger ReadBigInteger(this BinaryReader br)
        {
            var length = br.ReadInt32();
            return new BigInteger(br.ReadBytes(length));
        }
    }
}

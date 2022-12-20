using System.Security.Cryptography;

namespace ownable.dht;

public class XorShift
{
    private ulong _value1;
    private ulong _value2;

    public XorShift()
    {
        var seed = RandomNumberGenerator.GetBytes(16);
        _value1 = BitConverter.ToUInt64(seed, 0);
        _value2 = BitConverter.ToUInt64(seed, 8);
    }

    public int Next() => (int)NextUInt64();

    public ulong NextUInt64()
    {
        var result = _value1 + _value2;
        _value2 ^= _value1;
        _value1 = (((_value1 << 23) | (_value1 >> 41)) ^ _value2) ^ (result >> 4);
        _value2 ^= result << 4;
        return result + _value2;
    }
}
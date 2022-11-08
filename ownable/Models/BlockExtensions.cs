using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace ownable.Models
{
    internal static class BlockExtensions
    {
        public static BlockParameter ToBlockParameter(this BigInteger blockNumber)
        {
            return new BlockParameter(new HexBigInteger(blockNumber));
        }
    }
}

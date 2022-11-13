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

        public static string ToDisplayString(this BlockParameter blockParameter)
        {
            return blockParameter.ParameterType switch
            {
                BlockParameter.BlockParameterType.latest => "latest",
                BlockParameter.BlockParameterType.earliest => "earliest",
                BlockParameter.BlockParameterType.pending => "pending",
                BlockParameter.BlockParameterType.finalized => "finalized",
                BlockParameter.BlockParameterType.safe => "safe",
                BlockParameter.BlockParameterType.blockNumber => blockParameter.BlockNumber.Value.ToString(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}

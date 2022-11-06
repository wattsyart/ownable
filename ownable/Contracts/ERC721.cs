using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace ownable.Contracts
{
    public class ERC721 : ContractDeploymentMessage
    {
        [Parameter("string", "name")]
        public virtual string Name { get; set; } = null!;

        [Parameter("string", "symbol", 2)]
        public virtual string Symbol { get; set; } = null!;

        [Parameter("string", "baseTokenURI", 3)]
        public virtual string BaseTokenURI { get; set; } = null!;

        [Function("symbol", "string")]
        public class SymbolFunction : FunctionMessage { }

        [Function("name", "string")]
        public class NameFunction : FunctionMessage { }

        [Function("tokenURI", "string")]
        public class TokenURIFunction : FunctionMessage
        {
            [Parameter("uint256", "tokenId")]
            public BigInteger TokenId { get; set; }
        }

        [Function("supportsInterface", "bool")]
        public class SupportsInterfaceFunction : FunctionMessage
        {
            [Parameter("bytes4", "interfaceId")]
            public byte[] InterfaceId { get; set; } = null!;
        }

        [Event("Transfer")]
        public class Transfer : IEventDTO
        {
            [Parameter("address", "from", 1, true)]
            public string From { get; set; } = null!;

            [Parameter("address", "to", 2, true)]
            public string To { get; set; } = null!;

            [Parameter("uint256", "tokenId", 3, true)]
            public BigInteger TokenId { get; set; }
        }

        public ERC721() : base(string.Empty) { }
    }
}

using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace ownable.Contracts;

public class ERC1155 : ContractDeploymentMessage
{
    [Parameter("string", "name")]
    public virtual string Name { get; set; } = null!;

    [Parameter("string", "symbol", 2)]
    public virtual string Symbol { get; set; } = null!;

    [Function("name", "string")]
    public class NameFunction : FunctionMessage, ITokenNameFunction { }

    [Function("symbol", "string")]
    public class SymbolFunction : FunctionMessage, ITokenSymbolFunction { }

    [Function("uri", "string")]
    public class URIFunction : FunctionMessage, ITokenUriFunction
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    [Event("TransferBatch")]
    public class TransferBatch : ITokenEvent, ITransferEvent
    {
        [Parameter("address", "operator", 1, true)]
        public virtual string Operator { get; set; }
        [Parameter("address", "from", 2, true)]
        public virtual string From { get; set; }
        [Parameter("address", "to", 3, true)]
        public virtual string To { get; set; }
        [Parameter("uint256[]", "ids", 4, false)]
        public virtual List<BigInteger> Ids { get; set; }
        [Parameter("uint256[]", "values", 5, false)]
        public virtual List<BigInteger> Values { get; set; }

        public BigInteger GetTokenId() => Ids.FirstOrDefault();
    }

    [Event("TransferSingle")]
    public class TransferSingle : ITokenEvent, ITransferEvent
    {
        [Parameter("address", "operator", 1, true)]
        public virtual string Operator { get; set; }
        [Parameter("address", "from", 2, true)]
        public virtual string From { get; set; }
        [Parameter("address", "to", 3, true)]
        public virtual string To { get; set; }
        [Parameter("uint256", "id", 4, false)]
        public virtual BigInteger Id { get; set; }
        [Parameter("uint256", "value", 5, false)]
        public virtual BigInteger Value { get; set; }

        public BigInteger GetTokenId() => Id;
    }

    public ERC1155() : base(string.Empty) { }
}
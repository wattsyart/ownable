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
    public class NameFunction : FunctionMessage { }

    [Function("symbol", "string")]
    public class SymbolFunction : FunctionMessage { }

    [Event("TransferBatch")]
    public class TransferBatch : IEventDTO
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
    }

    [Event("TransferSingle")]
    public class TransferSingle : IEventDTO
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
    }

    public ERC1155() : base(string.Empty) { }
}
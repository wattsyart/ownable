using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace ownable.Contracts;

public class ERC165 : ContractDeploymentMessage
{
    [Function("supportsInterface", "bool")]
    public class SupportsInterfaceFunction : FunctionMessage
    {
        [Parameter("bytes4", "interfaceId")]
        public byte[] InterfaceId { get; set; } = null!;
    }

    public ERC165() : base(string.Empty) { }
}
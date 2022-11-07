using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ownable.Contracts;

public interface ITokenEvent : IEventDTO
{
    BigInteger GetTokenId();
}
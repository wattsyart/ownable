using System.Numerics;

namespace ownable.Contracts;

public interface ITokenUriFunction
{
    BigInteger TokenId { get; set; }
}
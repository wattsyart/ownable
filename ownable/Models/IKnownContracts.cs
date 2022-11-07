using ownable.Models.Indexed;

namespace ownable.Models;

public interface IKnownContracts
{
    bool TryGetContract(string contractAddress, out Contract? contract);
}
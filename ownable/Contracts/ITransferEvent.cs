using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ownable.Contracts;

public interface ITransferEvent : IEventDTO
{
    string From { get; }
    string To { get; }
}
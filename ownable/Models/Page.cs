using Nethereum.RPC.Eth.DTOs;

namespace ownable.Models
{
    public sealed class Page<T>
    {
        public List<T>? Value { get; set; }
        public string? NextPage { get; set; }
    }
}

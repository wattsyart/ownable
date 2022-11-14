using System.Numerics;
using ownable.Models.Indexed;

namespace ownable.Models
{
    public class CollectionItem
    {
        public BigInteger TokenId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ExternalUrl { get; set; }
        public IList<Trait> Traits { get; set; } = null!;
        public string? Media { get; set; }
    }
}

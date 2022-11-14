using ownable.Models.Indexed;

namespace ownable.Models
{
    public class CollectionItem
    {
        public IList<Trait> Traits { get; set; } = null!;
        public string? Media { get; set; }
    }
}

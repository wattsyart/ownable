namespace ownable.activitypub;

public class CollectionPage : Collection
{
    public ICollectionOrLink? PartOf { get; set; }
    public ICollectionOrLink? Next { get; set; }
    public ICollectionOrLink? Prev { get; set; }

}
namespace ownable.activitypub;

public class Collection : Object
{
    public ulong TotalItems { get; set; }
    public ICollectionPageOrLink? Current { get; set; }
    public ICollectionPageOrLink? First { get; set; }
    public ICollectionPageOrLink? Last { get; set; }
    public ICollectionPageOrLink? Items { get; set; }

}
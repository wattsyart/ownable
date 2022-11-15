namespace ownable.Models
{
    public struct IndexInfo
    {
        public string Name { get; set; }
        public ulong EntriesCount { get; set; }
        public ulong UsedSize { get; set; }
        public ulong TotalSize { get; set; }
    }
}

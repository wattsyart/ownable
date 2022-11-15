namespace ownable.Models;

public interface IIndex
{
    string Path { get; }
    ulong MapSize { get; }
    ulong GetMapSizeInUse();
    ulong GetEntriesCount();

    IndexInfo GetInfo();
}
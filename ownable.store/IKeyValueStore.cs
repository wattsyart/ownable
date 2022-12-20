namespace ownable.store;

public interface IKeyValueStore
{
    bool TryGet(ReadOnlySpan<byte> key, out ReadOnlySpan<byte> value);
    bool TryPut(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value);
}
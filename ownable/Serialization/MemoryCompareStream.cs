using System.Diagnostics;

namespace ownable.Serialization;

public sealed class MemoryCompareStream : Stream
{
    private readonly byte[] _comparand;
    private long _position;

    public MemoryCompareStream(byte[] comparand)
    {
        _comparand = comparand;
        _position = 0;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (buffer[offset + i] == _comparand[_position + i])
                continue;

            Debug.Assert(false);
            throw new Exception("Data mismatch");
        }

        _position += count;
    }

    public override void WriteByte(byte value)
    {
        if (_comparand[_position] != value)
        {
            Debug.Assert(false);
            throw new Exception("Data mismatch");
        }

        _position++;
    }


    public override bool CanRead => false;
    public override bool CanSeek => true;
    public override bool CanWrite => true;
    public override void Flush() { }
    public override long Length => _comparand.Length;
    public override long Position
    {
        get => _position; set => _position = value;
    }
    public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = offset;
                break;
            case SeekOrigin.Current:
                _position += offset;
                break;
            case SeekOrigin.End:
                _position = _comparand.Length - offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }
        return Position;
    }

    public override void SetLength(long value) => throw new InvalidOperationException();
}
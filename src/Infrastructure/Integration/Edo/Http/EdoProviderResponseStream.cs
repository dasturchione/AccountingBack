namespace Integration.Edo.Http;

internal sealed class EdoProviderResponseStream(Stream inner, HttpResponseMessage response, long maxLength) : Stream
{
    private readonly Stream _inner = inner;
    private readonly HttpResponseMessage _response = response;
    private readonly long _maxLength = maxLength;
    private long _read;

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
    public override void Flush() => _inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
    public override int Read(byte[] buffer, int offset, int count) => Check(_inner.Read(buffer, offset, Math.Min(count, Remaining())));
    public override int Read(Span<byte> buffer) => Check(_inner.Read(buffer[..Math.Min(buffer.Length, Remaining())]));
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => Check(await _inner.ReadAsync(buffer, offset, Math.Min(count, Remaining()), cancellationToken));
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => Check(await _inner.ReadAsync(buffer[..Math.Min(buffer.Length, Remaining())], cancellationToken));
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
    public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _inner.WriteAsync(buffer, offset, count, cancellationToken);
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _inner.WriteAsync(buffer, cancellationToken);

    private int Remaining() => (int)Math.Min(int.MaxValue, Math.Max(0, _maxLength - _read));

    private int Check(int count)
    {
        _read += count;
        if (_read > _maxLength)
            throw new InvalidDataException("The provider file exceeds the configured maximum size.");
        return count;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            _response.Dispose();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _inner.DisposeAsync();
        _response.Dispose();
        GC.SuppressFinalize(this);
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aviationexam.DependencyUpdater.Nuget.Tests.Infrastructure;

public class StreamProxy(Stream innerStream) : Stream
{
    public bool WasDisposed { get; private set; }
    public override bool CanRead => innerStream.CanRead;
    public override bool CanSeek => innerStream.CanSeek;
    public override bool CanWrite => innerStream.CanWrite;
    public override long Length => innerStream.Length;

    public override long Position
    {
        get => innerStream.Position;
        set => innerStream.Position = value;
    }

    public override void Flush() => innerStream.Flush();

    public override Task FlushAsync(
        CancellationToken cancellationToken
    ) => innerStream.FlushAsync(cancellationToken);

    public override int Read(
        byte[] buffer, int offset, int count
    ) => innerStream.Read(buffer, offset, count);

    public override long Seek(
        long offset, SeekOrigin origin
    ) => innerStream.Seek(offset, origin);

    public override void SetLength(
        long value
    ) => innerStream.SetLength(value);

    public override void Write(
        byte[] buffer, int offset, int count
    ) => innerStream.Write(buffer, offset, count);

    public override void Write(
        ReadOnlySpan<byte> buffer
    ) => innerStream.Write(buffer);

    public override Task WriteAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken
    ) => innerStream.WriteAsync(buffer, offset, count, cancellationToken);

    public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default
    ) => innerStream.WriteAsync(buffer, cancellationToken);

    public override void WriteByte(
        byte value
    ) => innerStream.WriteByte(value);

    public override ValueTask DisposeAsync()
    {
        WasDisposed = true;
        return base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        WasDisposed = true;
        base.Dispose(disposing);
    }
}

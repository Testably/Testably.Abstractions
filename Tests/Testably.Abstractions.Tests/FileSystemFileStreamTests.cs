using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileStreamTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    public abstract string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileStreamTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;

        Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void BeginRead_ShouldCopyContentsToBuffer(
        string path, byte[] contents)
    {
        ManualResetEventSlim ms = new();
        FileSystem.File.WriteAllBytes(path, contents);
        using FileSystemStream readStream = FileSystem.File.OpenRead(path);

        byte[] buffer = new byte[contents.Length];
        readStream.BeginRead(buffer, 0, buffer.Length, ar =>
        {
            // ReSharper disable once AccessToDisposedClosure
            readStream.EndRead(ar);
            ms.Set();
        }, null);

        ms.Wait(1000);
        buffer.Should().BeEquivalentTo(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void BeginWrite_ShouldCopyContentsToFile(
        string path, byte[] contents)
    {
        ManualResetEventSlim ms = new();
        using FileSystemStream writeStream = FileSystem.File.Create(path);

        writeStream.BeginWrite(contents, 0, contents.Length, ar =>
        {
            // ReSharper disable once AccessToDisposedClosure
            writeStream.EndWrite(ar);
            ms.Set();
        }, null);

        ms.Wait(1000);
        writeStream.Dispose();
        FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void CanSeek_ShouldReturnTrue(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        stream.CanSeek.Should().BeTrue();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void CanTimeout_ShouldReturnFalse(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        stream.CanTimeout.Should().BeFalse();
    }

    [SkippableFact]
    [FileSystemTests.FileStream]
    public void Constructor_EmptyPath_ShouldThrowArgumentException()
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New("", FileMode.Open);
        });

        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should().NotBeNullOrWhiteSpace();
    }

    [SkippableFact]
    [FileSystemTests.FileStream]
    public void Constructor_NullPath_ShouldThrowArgumentNullException()
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.FileStream.New(null!, FileMode.Open);
        });

        exception.Should().BeOfType<ArgumentNullException>()
           .Which.Message.Should().NotBeNullOrWhiteSpace();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void CopyTo_ShouldCopyBytes(
        string path, byte[] contents)
    {
        byte[] buffer = new byte[contents.Length];
        FileSystem.File.WriteAllBytes(path, contents);
        using FileSystemStream stream = FileSystem.File.OpenRead(path);
        using MemoryStream destination = new(buffer);

        stream.CopyTo(destination);

        destination.Flush();
        buffer.Should().BeEquivalentTo(contents);
    }

#if FEATURE_FILESYSTEM_ASYNC
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public async Task CopyToAsync_ShouldCopyBytes(
        string path, byte[] contents)
    {
        byte[] buffer = new byte[contents.Length];
        await FileSystem.File.WriteAllBytesAsync(path, contents);
        await using FileSystemStream stream = FileSystem.File.OpenRead(path);
        using MemoryStream destination = new(buffer);

        await stream.CopyToAsync(destination);

        await destination.FlushAsync();
        buffer.Should().BeEquivalentTo(contents);
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Name_ShouldReturnFullPath(string path)
    {
        string expectedName = FileSystem.Path.GetFullPath(path);
        using FileSystemStream stream = FileSystem.File.Create(path);

        stream.Name.Should().Be(expectedName);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Position_ShouldChangeWhenReading(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        stream.Position.Should().Be(0);
        stream.ReadByte();
        stream.Position.Should().Be(1);
    }

#if FEATURE_SPAN
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Read_AsSpan_ShouldFillBuffer(string path, byte[] contents)
    {
        byte[] buffer = new byte[contents.Length];
        FileSystem.File.WriteAllBytes(path, contents);
        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        int result = stream.Read(buffer.AsSpan());

        result.Should().Be(contents.Length);
        buffer.Should().BeEquivalentTo(contents);
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Read_ShouldFillBuffer(string path, byte[] contents)
    {
        byte[] buffer = new byte[contents.Length];
        FileSystem.File.WriteAllBytes(path, contents);
        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        int result = stream.Read(buffer, 0, contents.Length);

        result.Should().Be(contents.Length);
        buffer.Should().BeEquivalentTo(contents);
    }

#if FEATURE_FILESYSTEM_ASYNC
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public async Task ReadAsync_ShouldFillBuffer(string path, byte[] contents)
    {
        CancellationTokenSource cts = new(10000);
        byte[] buffer = new byte[contents.Length];
        await FileSystem.File.WriteAllBytesAsync(path, contents, cts.Token);
        await using FileSystemStream stream = FileSystem.File.OpenRead(path);

#pragma warning disable CA1835
        int result = await stream.ReadAsync(buffer, 0, contents.Length, cts.Token);
#pragma warning restore CA1835

        result.Should().Be(contents.Length);
        buffer.Should().BeEquivalentTo(contents);
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void ReadByte_ShouldReadSingleByteAndAdvancePosition(
        string path, byte[] contents)
    {
        FileSystem.File.WriteAllBytes(path, contents);

        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        int result1 = stream.ReadByte();
        int result2 = stream.ReadByte();

        stream.Position.Should().Be(2);
        result1.Should().Be(contents[0]);
        result2.Should().Be(contents[1]);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void ReadTimeout_ShouldThrowInvalidOperationException(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        Exception? exception = Record.Exception(() =>
        {
            using FileSystemStream stream = FileSystem.File.OpenRead(path);
            _ = stream.ReadTimeout;
        });

        exception.Should().BeOfType<InvalidOperationException>();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Seek_Begin_ShouldSetAbsolutePositionFromBegin(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        stream.Position.Should().Be(0);
        stream.Seek(4, SeekOrigin.Begin);
        stream.Position.Should().Be(4);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Seek_Current_ShouldSetRelativePosition(string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        stream.Position.Should().Be(0);
        stream.Seek(4, SeekOrigin.Current);
        stream.Position.Should().Be(4);
        stream.Seek(3, SeekOrigin.Current);
        stream.Position.Should().Be(7);
        stream.Seek(-1, SeekOrigin.Current);
        stream.Position.Should().Be(6);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Seek_End_ShouldSetAbsolutePositionFromEnd(string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        using FileSystemStream stream = FileSystem.File.OpenRead(path);

        stream.Position.Should().Be(0);
        stream.Seek(-4, SeekOrigin.End);
        stream.Position.Should().Be(contents.Length - 4);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void SetLength(string path, int length)
    {
        using FileSystemStream stream = FileSystem.File.Create(path);

        stream.SetLength(length);

        stream.Length.Should().Be(length);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void SetLength_ReadOnlyStream_ShouldThrowNotSupportedException(
        string path, int length)
    {
        FileSystem.File.WriteAllText(path, null);

        Exception? exception = Record.Exception(() =>
        {
            using FileSystemStream stream = FileSystem.File.OpenRead(path);
            stream.SetLength(length);
        });

        exception.Should().BeOfType<NotSupportedException>();
    }

#if FEATURE_SPAN
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Write_AsSpan_ShouldFillBuffer(string path, byte[] contents)
    {
        using FileSystemStream stream = FileSystem.File.Create(path);

        stream.Write(contents.AsSpan());

        stream.Dispose();
        FileSystem.File.ReadAllBytes(path)
           .Should().BeEquivalentTo(contents);
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void Write_ShouldFillBuffer(string path, byte[] contents)
    {
        using FileSystemStream stream = FileSystem.File.Create(path);

        stream.Write(contents, 0, contents.Length);

        stream.Dispose();
        FileSystem.File.ReadAllBytes(path)
           .Should().BeEquivalentTo(contents);
    }

#if FEATURE_FILESYSTEM_ASYNC
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public async Task WriteAsync_ShouldFillBuffer(string path, byte[] contents)
    {
        CancellationTokenSource cts = new(10000);
        await using FileSystemStream stream = FileSystem.File.Create(path);

#pragma warning disable CA1835
        await stream.WriteAsync(contents, 0, contents.Length, cts.Token);
#pragma warning restore CA1835

        await stream.DisposeAsync();
        (await FileSystem.File.ReadAllBytesAsync(path, cts.Token))
           .Should().BeEquivalentTo(contents);
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void WriteByte_ShouldWriteSingleByteAndAdvancePosition(
        string path, byte byte1, byte byte2)
    {
        using FileSystemStream stream = FileSystem.File.Create(path);

        stream.WriteByte(byte1);
        stream.WriteByte(byte2);

        stream.Position.Should().Be(2);
        stream.Dispose();
        FileSystem.File.ReadAllBytes(path)
           .Should().BeEquivalentTo(new[] { byte1, byte2 });
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileStream]
    public void WriteTimeout_ShouldThrowInvalidOperationException(
        string path, string contents)
    {
        FileSystem.File.WriteAllText(path, contents);

        Exception? exception = Record.Exception(() =>
        {
            using FileSystemStream stream = FileSystem.File.OpenRead(path);
            _ = stream.WriteTimeout;
        });

        exception.Should().BeOfType<InvalidOperationException>();
    }
}
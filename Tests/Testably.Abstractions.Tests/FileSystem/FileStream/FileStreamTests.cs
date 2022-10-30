using System.IO;
using System.Threading;
using Testably.Abstractions.FileSystem;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileStreamTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
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
	public void BeginWrite_ShouldCopyContentsToFile(
		string path, byte[] contents)
	{
		ManualResetEventSlim ms = new();
		using FileSystemStream writeStream = FileSystem.File.Create(path);
		writeStream.Flush();

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
	public void CanSeek_ShouldReturnTrue(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		stream.CanSeek.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CanTimeout_ShouldReturnFalse(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		stream.CanTimeout.Should().BeFalse();
	}

	[SkippableFact]
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
	public void Dispose_CalledTwiceShouldDoNothing(
		string path, byte[] contents)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.File.WriteAllBytes(path, contents);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.ReadWrite, 10, FileOptions.DeleteOnClose);

		stream.Dispose();
		FileSystem.File.Exists(path).Should().BeFalse();
		FileSystem.File.WriteAllText(path, "foo");

		stream.Dispose();

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Dispose_ShouldDisposeStream(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using Stream stream = FileSystem.File.OpenRead(path);
		stream.Dispose();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.ReadByte();
		});

		exception.Should().BeOfType<ObjectDisposedException>();
	}

	[SkippableTheory]
	[AutoData]
	public void EndRead_Null_ShouldThrowArgumentNullException(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);
		using FileSystemStream readStream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			readStream.EndRead(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

	[SkippableTheory]
	[AutoData]
	public void EndRead_ShouldNotAdjustTimes(string path, byte[] contents)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		ManualResetEventSlim ms = new();
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		using FileSystemStream readStream = FileSystem.File.OpenRead(path);
		DateTime updateTime = DateTime.MinValue;

		byte[] buffer = new byte[contents.Length];
		readStream.BeginRead(buffer, 0, buffer.Length, ar =>
		{
			TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
			updateTime = TimeSystem.DateTime.UtcNow;
			// ReSharper disable once AccessToDisposedClosure
			readStream.EndRead(ar);
			ms.Set();
		}, null);

		ms.Wait(10000);
		readStream.Dispose();

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		creationTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}

	[SkippableTheory]
	[AutoData]
	public void EndWrite_Null_ShouldThrowArgumentNullException(string path)
	{
		using FileSystemStream writeStream = FileSystem.File.Create(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			writeStream.EndWrite(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

	[SkippableTheory]
	[AutoData]
	public void EndWrite_ShouldAdjustTimes(string path, byte[] contents)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		ManualResetEventSlim ms = new();
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		using FileSystemStream writeStream = FileSystem.File.Create(path);
		DateTime updateTime = DateTime.MinValue;

		writeStream.BeginWrite(contents, 0, contents.Length, ar =>
		{
			TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
			updateTime = TimeSystem.DateTime.UtcNow;
			// ReSharper disable once AccessToDisposedClosure
			writeStream.EndWrite(ar);
			ms.Set();
		}, null);

		ms.Wait(10000);
		writeStream.Dispose();

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
			   .BeOnOrAfter(updateTime);
		}
		else
		{
			lastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
		   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[SkippableTheory]
	[AutoData]
	public void ExtensionContainer_ShouldWrapFileStreamOnRealFileSystem(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		using FileSystemStream readStream = FileSystem.File.OpenRead(path);
		bool result = readStream.ExtensionContainer
		   .HasWrappedInstance(out System.IO.FileStream? fileStream);

		if (FileSystem is RealFileSystem)
		{
			result.Should().BeTrue();
			fileStream!.Name.Should().Be(readStream.Name);
		}
		else
		{
			result.Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void Flush_ShouldNotChangePosition(
		string path, byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Write(bytes, 0, bytes.Length);
		stream.Seek(2, SeekOrigin.Begin);
		stream.Position.Should().Be(2);

		stream.Flush();

		stream.Position.Should().Be(2);
	}

	[SkippableTheory]
	[AutoData]
	public void Flush_ShouldNotUpdateFileContentWhenAlreadyFlushed(
		string path, byte[] bytes1, byte[] bytes2)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		using FileSystemStream stream1 = FileSystem.File.Open(
			path,
			FileMode.OpenOrCreate,
			FileAccess.ReadWrite,
			FileShare.ReadWrite);
		using FileSystemStream stream2 = FileSystem.File.Open(
			path,
			FileMode.OpenOrCreate,
			FileAccess.ReadWrite,
			FileShare.ReadWrite);
		stream1.Write(bytes1, 0, bytes1.Length);
		stream1.Flush();
		stream2.Write(bytes2, 0, bytes2.Length);
		stream2.Flush();

		stream1.Flush();

		stream2.Dispose();
		stream1.Dispose();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes2);
	}

	[SkippableTheory]
	[AutoData]
	public void Name_ShouldReturnFullPath(string path)
	{
		string expectedName = FileSystem.Path.GetFullPath(path);
		using FileSystemStream stream = FileSystem.File.Create(path);

		stream.Name.Should().Be(expectedName);
	}

	[SkippableTheory]
	[AutoData]
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
	public void SetLength(string path, int length)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		stream.SetLength(length);

		stream.Length.Should().Be(length);
	}

	[SkippableTheory]
	[AutoData]
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
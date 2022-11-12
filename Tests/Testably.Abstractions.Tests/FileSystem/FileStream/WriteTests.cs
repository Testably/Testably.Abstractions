using System.Threading;
using Testably.Abstractions.FileSystem;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WriteTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void BeginWrite_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);
		FileSystemStream stream = FileSystem.FileInfo.New(path).OpenRead();

		byte[] buffer = new byte[contents.Length];
		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.BeginWrite(buffer, 0, buffer.Length, _ => { }, null);
		});

		stream.Dispose();

		exception.Should().BeOfType<NotSupportedException>()
		   .Which.HResult.Should().Be(-2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public void BeginWrite_ShouldCopyContentsToFile(
		string path, byte[] contents)
	{
		ManualResetEventSlim ms = new();
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Flush();

		stream.BeginWrite(contents, 0, contents.Length, ar =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndWrite(ar);
			ms.Set();
		}, null);

		ms.Wait(1000);
		stream.Dispose();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void EndWrite_Null_ShouldThrowArgumentNullException(string path)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndWrite(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.HResult.Should().Be(-2147467261);
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
		using FileSystemStream stream = FileSystem.File.Create(path);
		DateTime updateTime = DateTime.MinValue;

		stream.BeginWrite(contents, 0, contents.Length, ar =>
		{
			TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
			updateTime = TimeSystem.DateTime.UtcNow;
			// ReSharper disable once AccessToDisposedClosure
			stream.EndWrite(ar);
			ms.Set();
		}, null);

		ms.Wait(10000);
		stream.Dispose();

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
	public void Write_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		byte[] buffer = new byte[contents.Length];
		FileSystem.File.WriteAllBytes(path, contents);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.Write(buffer, 0, contents.Length);
		});

		stream.Dispose();

		exception.Should().BeOfType<NotSupportedException>()
		   .Which.HResult.Should().Be(-2146233067);
	}

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

	[SkippableTheory]
	[AutoData]
	public void WriteByte_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.WriteByte(0);
		});

		stream.Dispose();

		exception.Should().BeOfType<NotSupportedException>()
		   .Which.HResult.Should().Be(-2146233067);
	}

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

		exception.Should().BeOfType<InvalidOperationException>()
		   .Which.HResult.Should().Be(-2146233079);
		exception.Should().BeOfType<InvalidOperationException>();
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

	[SkippableTheory]
	[AutoData]
	public void Write_AsSpan_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		byte[] buffer = new byte[contents.Length];
		FileSystem.File.WriteAllBytes(path, contents);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.Write(buffer.AsSpan());
		});

		stream.Dispose();

		exception.Should().BeOfType<NotSupportedException>()
		   .Which.HResult.Should().Be(-2146233067);
	}
#endif

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

	[SkippableTheory]
	[AutoData]
	public async Task WriteAsync_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		CancellationTokenSource cts = new(10000);
		byte[] buffer = new byte[contents.Length];
		await FileSystem.File.WriteAllBytesAsync(path, contents, cts.Token);
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			// ReSharper disable once AccessToDisposedClosure
#pragma warning disable CA1835
			await stream.WriteAsync(buffer, 0, contents.Length, cts.Token);
#pragma warning restore CA1835
		});

		await stream.DisposeAsync();

		exception.Should().BeOfType<NotSupportedException>()
		   .Which.HResult.Should().Be(-2146233067);
	}
#endif
}
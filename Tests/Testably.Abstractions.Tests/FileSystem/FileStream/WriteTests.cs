using System.IO;
using System.Threading;
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
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystemStream stream = FileSystem.FileInfo.New(path).OpenRead();

		byte[] buffer = new byte[bytes.Length];
		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.BeginWrite(buffer, 0, buffer.Length, _ => { }, null);
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public void BeginWrite_ShouldCopyContentsToFile(
		string path, byte[] bytes)
	{
		ManualResetEventSlim ms = new();
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Flush();

		stream.BeginWrite(bytes, 0, bytes.Length, ar =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndWrite(ar);
			ms.Set();
		}, null);

		ms.Wait(30000);
		stream.Dispose();
		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
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

		exception.Should().BeException<ArgumentNullException>(hResult: -2147467261);
	}

	[SkippableTheory]
	[AutoData]
	public void EndWrite_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		ManualResetEventSlim ms = new();
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.Create(path);
		DateTime updateTime = DateTime.MinValue;

		stream.BeginWrite(bytes, 0, bytes.Length, ar =>
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
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.Write(buffer, 0, bytes.Length);
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public void Write_ShouldFillBuffer(string path, byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		stream.Write(bytes, 0, bytes.Length);

		stream.Dispose();
		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteByte_HiddenFile_ShouldNotThrow(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.WriteByte(0);
		});

		stream.Dispose();

		exception.Should().BeNull();
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
		FileSystem.Should().HaveFile(path)
			.Which.HasContent(new[]
			{
				byte1, byte2
			});
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

		exception.Should().BeException<InvalidOperationException>(
			hResult: -2146233079);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void Write_AsSpan_ShouldFillBuffer(string path, byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		stream.Write(bytes.AsSpan());

		stream.Dispose();
		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public void Write_AsSpan_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.Write(buffer.AsSpan());
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableTheory]
	[AutoData]
	public async Task WriteAsync_ShouldFillBuffer(string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(30000);
		await using FileSystemStream stream = FileSystem.File.Create(path);

		#pragma warning disable CA1835
		await stream.WriteAsync(bytes, 0, bytes.Length, cts.Token);
		#pragma warning restore CA1835

		await stream.DisposeAsync();
		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAsync_CanWriteFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(30000);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			// ReSharper disable once AccessToDisposedClosure
			#pragma warning disable CA1835
			await stream.WriteAsync(buffer, 0, bytes.Length, cts.Token);
			#pragma warning restore CA1835
		});

		await stream.DisposeAsync();

		exception.Should().BeException<NotSupportedException>(
			hResult: -2146233067);
	}
#endif
}

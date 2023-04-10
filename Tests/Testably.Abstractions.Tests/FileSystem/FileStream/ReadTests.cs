using System.Threading;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReadTests<TFileSystem>
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
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		byte[] buffer = new byte[contents.Length];
		stream.BeginRead(buffer, 0, buffer.Length, ar =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndRead(ar);
			ms.Set();
		}, null);

		ms.Wait(30000);
		buffer.Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void BeginRead_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);
		FileSystemStream stream = FileSystem.FileInfo.New(path).OpenWrite();

		byte[] buffer = new byte[contents.Length];
		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.BeginRead(buffer, 0, buffer.Length, _ => { }, null);
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public void EndRead_Null_ShouldThrowArgumentNullException(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndRead(null!);
		});

		exception.Should().BeException<ArgumentNullException>(hResult: -2147467261);
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
		using FileSystemStream stream = FileSystem.File.OpenRead(path);
		DateTime updateTime = DateTime.MinValue;

		byte[] buffer = new byte[contents.Length];
		stream.BeginRead(buffer, 0, buffer.Length, ar =>
		{
			TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
			updateTime = TimeSystem.DateTime.UtcNow;
			// ReSharper disable once AccessToDisposedClosure
			stream.EndRead(ar);
			ms.Set();
		}, null);

		ms.Wait(10000);
		stream.Dispose();

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
	public void Read_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		byte[] buffer = new byte[contents.Length];
		FileSystem.File.WriteAllBytes(path, contents);
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			_ = stream.Read(buffer, 0, contents.Length);
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

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

	[SkippableTheory]
	[AutoData]
	public void ReadByte_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			_ = stream.ReadByte();
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

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

		exception.Should().BeException<InvalidOperationException>(hResult: -2146233079);
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

	[SkippableTheory]
	[AutoData]
	public void Read_AsSpan_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		byte[] buffer = new byte[contents.Length];
		FileSystem.File.WriteAllBytes(path, contents);
		using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			_ = stream.Read(buffer.AsSpan());
		});

		stream.Dispose();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_ShouldFillBuffer(string path, byte[] contents)
	{
		using CancellationTokenSource cts = new(30000);
		byte[] buffer = new byte[contents.Length];
		await FileSystem.File.WriteAllBytesAsync(path, contents, cts.Token);
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

		#pragma warning disable CA1835
		int result = await stream.ReadAsync(buffer, 0, contents.Length, cts.Token);
		#pragma warning restore CA1835

		result.Should().Be(contents.Length);
		buffer.Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		using CancellationTokenSource cts = new(30000);
		byte[] buffer = new byte[contents.Length];
		await FileSystem.File.WriteAllBytesAsync(path, contents, cts.Token);
		await using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			// ReSharper disable once AccessToDisposedClosure
			#pragma warning disable CA1835
			_ = await stream.ReadAsync(buffer, 0, contents.Length, cts.Token);
			#pragma warning restore CA1835
		});

		await stream.DisposeAsync();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_Memory_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] contents)
	{
		using CancellationTokenSource cts = new(30000);
		byte[] buffer = new byte[contents.Length];
		await FileSystem.File.WriteAllBytesAsync(path, contents, cts.Token);
		await using FileSystemStream stream = FileSystem.File.OpenWrite(path);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			// ReSharper disable once AccessToDisposedClosure
			#pragma warning disable CA1835
			_ = await stream.ReadAsync(buffer.AsMemory(), cts.Token);
			#pragma warning restore CA1835
		});

		await stream.DisposeAsync();

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif
}

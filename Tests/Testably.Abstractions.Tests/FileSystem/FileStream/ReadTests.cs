using System.Threading;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReadTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void BeginRead_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystemStream stream = FileSystem.FileInfo.New(path).OpenWrite();

		byte[] buffer = new byte[bytes.Length];
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
	public void BeginRead_ShouldCopyContentsToBuffer(
		string path, byte[] bytes)
	{
		using ManualResetEventSlim ms = new();
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		byte[] buffer = new byte[bytes.Length];
		stream.BeginRead(buffer, 0, buffer.Length, ar =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.EndRead(ar);
			ms.Set();
		}, null);

		ms.Wait(30000);
		buffer.Should().BeEquivalentTo(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public void EndRead_Null_ShouldThrowArgumentNullException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
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
	public void EndRead_ShouldNotAdjustTimes(string path, byte[] bytes)
	{
		SkipIfBrittleTestsShouldBeSkipped(Test.RunsOnMac);

		using ManualResetEventSlim ms = new();
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		DateTime updateTime = DateTime.MinValue;

		using (FileSystemStream stream = FileSystem.File.OpenRead(path))
		{
			byte[] buffer = new byte[bytes.Length];
			stream.BeginRead(buffer, 0, buffer.Length, ar =>
			{
				TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
				// ReSharper disable once AccessToDisposedClosure
				stream.EndRead(ar);
				updateTime = TimeSystem.DateTime.UtcNow;
				ms.Set();
			}, null);

			ms.Wait(10000);
		}

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		creationTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
		lastAccessTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
		lastWriteTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void Read_AsSpan_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		Exception? exception;

		using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			exception = Record.Exception(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				_ = stream.Read(buffer.AsSpan());
			});
		}

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

#if FEATURE_SPAN
	[SkippableTheory]
	[AutoData]
	public void Read_AsSpan_ShouldFillBuffer(string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		int result = stream.Read(buffer.AsSpan());

		result.Should().Be(bytes.Length);
		buffer.Should().BeEquivalentTo(bytes);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void Read_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		Exception? exception;

		using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			exception = Record.Exception(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				_ = stream.Read(buffer, 0, bytes.Length);
			});
		}

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public void Read_ShouldFillBuffer(string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		int result = stream.Read(buffer, 0, bytes.Length);

		result.Should().Be(bytes.Length);
		buffer.Should().BeEquivalentTo(bytes);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(30000);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);
		Exception? exception;

		await using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			exception = await Record.ExceptionAsync(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				#pragma warning disable CA1835
				_ = await stream.ReadAsync(buffer, 0, bytes.Length, cts.Token);
				#pragma warning restore CA1835
			});
		}

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_Memory_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(30000);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);
		Exception? exception;

		await using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			exception = await Record.ExceptionAsync(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				#pragma warning disable CA1835
				_ = await stream.ReadAsync(buffer.AsMemory(), cts.Token);
				#pragma warning restore CA1835
			});
		}

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[SkippableTheory]
	[AutoData]
	public async Task ReadAsync_ShouldFillBuffer(string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new(30000);
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);

		#pragma warning disable CA1835
		int result = await stream.ReadAsync(buffer, 0, bytes.Length, cts.Token);
		#pragma warning restore CA1835

		result.Should().Be(bytes.Length);
		buffer.Should().BeEquivalentTo(bytes);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void ReadByte_CanReadFalse_ShouldThrowNotSupportedException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		Exception? exception;

		using (FileSystemStream stream = FileSystem.File.OpenWrite(path))
		{
			exception = Record.Exception(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				_ = stream.ReadByte();
			});
		}

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}

	[SkippableTheory]
	[AutoData]
	public void ReadByte_ShouldReadSingleByteAndAdvancePosition(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		int result1 = stream.ReadByte();
		int result2 = stream.ReadByte();

		stream.Position.Should().Be(2);
		result1.Should().Be(bytes[0]);
		result2.Should().Be(bytes[1]);
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
}

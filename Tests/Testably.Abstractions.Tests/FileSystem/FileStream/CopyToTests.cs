using System.IO;
#if FEATURE_SPAN
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public class CopyToTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task CopyTo_BufferSizeZero_ShouldThrowArgumentOutOfRangeException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);

		void Act()
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			using MemoryStream destination = new(buffer);
			stream.CopyTo(destination, 0);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("bufferSize");
	}

	[Test]
	[AutoArguments]
	public async Task CopyTo_ShouldCopyBytes(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);
		using MemoryStream destination = new(buffer);

		stream.CopyTo(destination);

		destination.Flush();
		await That(buffer).IsEqualTo(bytes).InAnyOrder();
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	[AutoArguments]
	public async Task CopyToAsync_BufferSizeZero_ShouldThrowArgumentOutOfRangeException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, CancellationToken);

		async Task Act()
		{
			await using FileSystemStream stream = FileSystem.File.OpenRead(path);
			using MemoryStream destination = new(buffer);
			await stream.CopyToAsync(destination, 0, CancellationToken);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("bufferSize");
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	[AutoArguments]
	public async Task CopyToAsync_ShouldCopyBytes(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, CancellationToken);
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);
		using MemoryStream destination = new(buffer);

		await stream.CopyToAsync(destination, CancellationToken);

		await destination.FlushAsync(CancellationToken);
		await That(buffer).IsEqualTo(bytes).InAnyOrder();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	[Arguments(0)]
	[Arguments(-1)]
	public async Task CopyToAsync_WhenBufferSizeIsNotPositive_ShouldThrowArgumentNullException(
		int bufferSize)
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenWrite();

		async Task Act()
		{
			await source.CopyToAsync(destination, bufferSize, CancellationToken);
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	public async Task CopyToAsync_WhenDestinationIsClosed_ShouldThrowObjectDisposedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		using MemoryStream destination = new();
		destination.Close();

		async Task Act()
		{
			await source.CopyToAsync(destination, CancellationToken);
		}

		await That(Act).ThrowsExactly<ObjectDisposedException>().WithMessage("Cannot access a*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	public async Task CopyToAsync_WhenDestinationIsNull_ShouldThrowArgumentNullException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();

		async Task Act()
		{
			await source.CopyToAsync(null!, CancellationToken);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithMessage("*cannot be null*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	public async Task CopyToAsync_WhenDestinationIsReadOnly_ShouldThrowNotSupportedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenRead();

		async Task Act()
		{
			await source.CopyToAsync(destination, CancellationToken);
		}

		await That(Act).ThrowsExactly<NotSupportedException>().WithMessage("Stream does not support writing*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	public async Task CopyToAsync_WhenSourceIsClosed_ShouldThrowObjectDisposedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenWrite();
		source.Close();

		async Task Act()
		{
			await source.CopyToAsync(destination, CancellationToken);
		}

		await That(Act).ThrowsExactly<ObjectDisposedException>().WithMessage("Cannot access a*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Test]
	public async Task CopyToAsync_WhenSourceIsWriteOnly_ShouldThrowNotSupportedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenWrite();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenWrite();

		async Task Act()
		{
			await source.CopyToAsync(destination, CancellationToken);
		}

		await That(Act).ThrowsExactly<NotSupportedException>().WithMessage("Stream does not support reading*").AsWildcard();
	}
#endif
}

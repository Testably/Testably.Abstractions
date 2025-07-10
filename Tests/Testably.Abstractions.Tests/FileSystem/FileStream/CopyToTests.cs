using System.IO;
#if FEATURE_SPAN
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class CopyToTests
{
	[Theory]
	[AutoData]
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

	[Theory]
	[AutoData]
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
	[Theory]
	[AutoData]
	public async Task CopyToAsync_BufferSizeZero_ShouldThrowArgumentOutOfRangeException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);

		async Task Act()
		{
			await using FileSystemStream stream = FileSystem.File.OpenRead(path);
			using MemoryStream destination = new(buffer);
			await stream.CopyToAsync(destination, 0, TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("bufferSize");
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[AutoData]
	public async Task CopyToAsync_ShouldCopyBytes(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
		await using FileSystemStream stream = FileSystem.File.OpenRead(path);
		using MemoryStream destination = new(buffer);

		await stream.CopyToAsync(destination, TestContext.Current.CancellationToken);

		await destination.FlushAsync(TestContext.Current.CancellationToken);
		await That(buffer).IsEqualTo(bytes).InAnyOrder();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task CopyToAsync_WhenBufferSizeIsNotPositive_ShouldThrowArgumentNullException(
		int bufferSize)
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", TestContext.Current.CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", TestContext.Current.CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenWrite();

		async Task Act()
		{
			await source.CopyToAsync(destination, bufferSize, TestContext.Current.CancellationToken);
		}

		await That(Act).ThrowsExactly<ArgumentOutOfRangeException>();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task CopyToAsync_WhenDestinationIsClosed_ShouldThrowObjectDisposedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", TestContext.Current.CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		using MemoryStream destination = new();
		destination.Close();

		async Task Act()
		{
			await source.CopyToAsync(destination, TestContext.Current.CancellationToken);
		}

		await That(Act).ThrowsExactly<ObjectDisposedException>().WithMessage("Cannot access a*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task CopyToAsync_WhenDestinationIsNull_ShouldThrowArgumentNullException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", TestContext.Current.CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();

		async Task Act()
		{
			await source.CopyToAsync(null!, TestContext.Current.CancellationToken);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithMessage("*cannot be null*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task CopyToAsync_WhenDestinationIsReadOnly_ShouldThrowNotSupportedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", TestContext.Current.CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", TestContext.Current.CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenRead();

		async Task Act()
		{
			await source.CopyToAsync(destination, TestContext.Current.CancellationToken);
		}

		await That(Act).ThrowsExactly<NotSupportedException>().WithMessage("Stream does not support writing*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task CopyToAsync_WhenSourceIsClosed_ShouldThrowObjectDisposedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", TestContext.Current.CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", TestContext.Current.CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenRead();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenWrite();
		source.Close();

		async Task Act()
		{
			await source.CopyToAsync(destination, TestContext.Current.CancellationToken);
		}

		await That(Act).ThrowsExactly<ObjectDisposedException>().WithMessage("Cannot access a*").AsWildcard();
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	[Fact]
	public async Task CopyToAsync_WhenSourceIsWriteOnly_ShouldThrowNotSupportedException()
	{
		await FileSystem.File.WriteAllTextAsync("foo.txt", "", TestContext.Current.CancellationToken);
		await FileSystem.File.WriteAllTextAsync("bar.txt", "", TestContext.Current.CancellationToken);
		await using FileSystemStream source = FileSystem.FileInfo.New("foo.txt").OpenWrite();
		await using FileSystemStream destination = FileSystem.FileInfo.New("bar.txt").OpenWrite();

		async Task Act()
		{
			await source.CopyToAsync(destination, TestContext.Current.CancellationToken);
		}

		await That(Act).ThrowsExactly<NotSupportedException>().WithMessage("Stream does not support reading*").AsWildcard();
	}
#endif
}

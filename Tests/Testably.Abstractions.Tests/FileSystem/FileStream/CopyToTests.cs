using System.IO;
using System.Threading.Tasks;
#if FEATURE_SPAN
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class CopyToTests
{
	[Theory]
	[AutoData]
	public void CopyTo_BufferSizeZero_ShouldThrowArgumentOutOfRangeException(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			using MemoryStream destination = new(buffer);
			stream.CopyTo(destination, 0);
		});

		exception.Should().BeException<ArgumentOutOfRangeException>(
			paramName: "bufferSize");
	}

	[Theory]
	[AutoData]
	public void CopyTo_ShouldCopyBytes(
		string path, byte[] bytes)
	{
		byte[] buffer = new byte[bytes.Length];
		FileSystem.File.WriteAllBytes(path, bytes);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);
		using MemoryStream destination = new(buffer);

		stream.CopyTo(destination);

		destination.Flush();
		buffer.Should().BeEquivalentTo(bytes);
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
			await stream.CopyToAsync(destination, 0);
		}
		
		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<ArgumentOutOfRangeException>(
			paramName: "bufferSize");
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
		buffer.Should().BeEquivalentTo(bytes);
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
			await source.CopyToAsync(destination, bufferSize);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeOfType<ArgumentOutOfRangeException>();
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
			await source.CopyToAsync(destination);
		}
		
		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeOfType<ObjectDisposedException>()
			.Which.Message.Should().Match("Cannot access a*");
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
			await source.CopyToAsync(null!);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.Message.Should().Match("*cannot be null*");
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
			await source.CopyToAsync(destination);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should().Match("Stream does not support writing*");
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
			await source.CopyToAsync(destination);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeOfType<ObjectDisposedException>()
			.Which.Message.Should().Match("Cannot access a*");
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
			await source.CopyToAsync(destination);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeOfType<NotSupportedException>()
			.Which.Message.Should().Match("Stream does not support reading*");
	}
#endif
}

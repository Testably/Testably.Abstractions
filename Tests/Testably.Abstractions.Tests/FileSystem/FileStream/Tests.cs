using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
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

	[SkippableTheory]
	[AutoData]
	public void Close_CalledMultipleTimes_ShouldNotThrow(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);
		using FileSystemStream stream = FileSystem.File.OpenRead(path);
		stream.Close();

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			stream.Close();
		});

		exception.Should().BeNull();
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

	[SkippableTheory]
	[AutoData]
	public void CopyTo_BufferSizeZero_ShouldThrowArgumentOutOfRangeException(
		string path, byte[] contents)
	{
		byte[] buffer = new byte[contents.Length];
		FileSystem.File.WriteAllBytes(path, contents);

		Exception? exception = Record.Exception(() =>
		{
			using FileSystemStream stream = FileSystem.File.OpenRead(path);
			using MemoryStream destination = new(buffer);
			stream.CopyTo(destination, 0);
		});

		exception.Should().BeException<ArgumentOutOfRangeException>(
			paramName: "bufferSize");
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

	[SkippableTheory]
	[AutoData]
	public async Task CopyToAsync_BufferSizeZero_ShouldThrowArgumentOutOfRangeException(
		string path, byte[] contents)
	{
		byte[] buffer = new byte[contents.Length];
		await FileSystem.File.WriteAllBytesAsync(path, contents);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await using FileSystemStream stream = FileSystem.File.OpenRead(path);
			using MemoryStream destination = new(buffer);
			await stream.CopyToAsync(destination, 0);
		});

		exception.Should().BeException<ArgumentOutOfRangeException>(
			paramName: "bufferSize");
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void Extensibility_ShouldWrapFileStreamOnRealFileSystem(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		using FileSystemStream readStream = FileSystem.File.OpenRead(path);
		bool result = readStream.Extensibility
			.TryGetWrappedInstance(out System.IO.FileStream? fileStream);

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
	public async Task FlushAsync_ShouldNotChangePosition(
		string path, byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Write(bytes, 0, bytes.Length);
		stream.Seek(2, SeekOrigin.Begin);
		stream.Position.Should().Be(2);

		await stream.FlushAsync();

		stream.Position.Should().Be(2);
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

		exception.Should().BeException<NotSupportedException>(hResult: -2146233067);
	}
}

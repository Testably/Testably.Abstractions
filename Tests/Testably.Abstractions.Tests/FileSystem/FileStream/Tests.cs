using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Helpers;
#if FEATURE_SPAN
using System.Text;
#endif

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public void CanSeek_ShouldReturnTrue(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		stream.CanSeek.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void CanTimeout_ShouldReturnFalse(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		stream.CanTimeout.Should().BeFalse();
	}

	[Theory]
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

	[Theory]
	[AutoData]
	public void Extensibility_ShouldWrapFileStreamOnRealFileSystem(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		using FileSystemStream readStream = FileSystem.File.OpenRead(path);
		IFileSystemExtensibility? extensibility = readStream as IFileSystemExtensibility;
		bool result = extensibility?.TryGetWrappedInstance(out System.IO.FileStream? fileStream)
		              ?? throw new NotSupportedException(
			              $"{readStream.GetType()} does not implement IFileSystemExtensibility");

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

	[Theory]
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

	[Theory]
	[AutoData]
	public void Flush_ShouldNotUpdateFileContentWhenAlreadyFlushed(
		string path, byte[] bytes1, byte[] bytes2)
	{
		using (FileSystemStream stream1 = FileSystem.File.Open(
			path,
			FileMode.OpenOrCreate,
			FileAccess.ReadWrite,
			FileShare.ReadWrite))
		{
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
		}

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes2);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public void Flush_WriteToDisk_ShouldNotChangePosition(
		bool flushToDisk, string path, byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Write(bytes, 0, bytes.Length);
		stream.Seek(2, SeekOrigin.Begin);
		stream.Position.Should().Be(2);

		stream.Flush(flushToDisk);

		stream.Position.Should().Be(2);
	}

	[Theory]
	[AutoData]
	public async Task FlushAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act()
		{
			// ReSharper disable once UseAwaitUsing
			using FileSystemStream stream = FileSystem.File.Create(path);
			await stream.FlushAsync(cts.Token);
		}
		
		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task FlushAsync_ShouldNotChangePosition(
		string path, byte[] bytes)
	{
		// ReSharper disable once UseAwaitUsing
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Write(bytes, 0, bytes.Length);
		stream.Seek(2, SeekOrigin.Begin);
		stream.Position.Should().Be(2);

		await stream.FlushAsync(TestContext.Current.CancellationToken);

		stream.Position.Should().Be(2);
	}

	[Theory]
	[AutoData]
	public void Name_ShouldReturnFullPath(string path)
	{
		string expectedName = FileSystem.Path.GetFullPath(path);
		using FileSystemStream stream = FileSystem.File.Create(path);

		stream.Name.Should().Be(expectedName);
	}

	[Theory]
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
	[Theory]
	[AutoData]
	public void Position_ShouldNotChangeSharedBufferStreamsWhenWriting(
		string path, string contents, string changedContents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream fileStream1 = FileSystem.FileStream.New(
			path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		using FileSystemStream fileStream2 = FileSystem.FileStream.New(
			path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		long initialPosition1 = 3L;
		long initialPosition2 = 4L;

		fileStream1.Position = initialPosition1;
		fileStream2.Position = initialPosition2;

		fileStream1.Write(Encoding.UTF8.GetBytes(changedContents));

		fileStream1.Position.Should().Be(initialPosition1 + changedContents.Length);
		fileStream2.Position.Should().Be(initialPosition2);

		fileStream1.Flush();

		fileStream1.Position.Should().Be(initialPosition1 + changedContents.Length);
		fileStream2.Position.Should().Be(initialPosition2);
	}
#endif

	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public void Seek_End_ShouldSetAbsolutePositionFromEnd(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		stream.Position.Should().Be(0);
		stream.Seek(-4, SeekOrigin.End);
		stream.Position.Should().Be(contents.Length - 4);
	}

	[Theory]
	[AutoData]
	public void SetLength(string path, int length)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		stream.SetLength(length);

		stream.Length.Should().Be(length);
	}

	[Theory]
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

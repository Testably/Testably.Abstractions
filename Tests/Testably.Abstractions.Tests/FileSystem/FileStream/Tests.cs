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
	public async Task CanSeek_ShouldReturnTrue(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		await That(stream.CanSeek).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task CanTimeout_ShouldReturnFalse(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		await That(stream.CanTimeout).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Close_CalledMultipleTimes_ShouldNotThrow(
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

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task Extensibility_ShouldWrapFileStreamOnRealFileSystem(
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
			await That(result).IsTrue();
			await That(fileStream!.Name).IsEqualTo(readStream.Name);
		}
		else
		{
			await That(result).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task Flush_ShouldNotChangePosition(
		string path, byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Write(bytes, 0, bytes.Length);
		stream.Seek(2, SeekOrigin.Begin);
		await That(stream.Position).IsEqualTo(2);

		stream.Flush();

		await That(stream.Position).IsEqualTo(2);
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

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes2);
	}

	[Theory]
	[InlineAutoData(false)]
	[InlineAutoData(true)]
	public async Task Flush_WriteToDisk_ShouldNotChangePosition(
		bool flushToDisk, string path, byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);
		stream.Write(bytes, 0, bytes.Length);
		stream.Seek(2, SeekOrigin.Begin);
		await That(stream.Position).IsEqualTo(2);

		stream.Flush(flushToDisk);

		await That(stream.Position).IsEqualTo(2);
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
		await That(stream.Position).IsEqualTo(2);

		await stream.FlushAsync(TestContext.Current.CancellationToken);

		await That(stream.Position).IsEqualTo(2);
	}

	[Theory]
	[AutoData]
	public async Task Name_ShouldReturnFullPath(string path)
	{
		string expectedName = FileSystem.Path.GetFullPath(path);
		using FileSystemStream stream = FileSystem.File.Create(path);

		await That(stream.Name).IsEqualTo(expectedName);
	}

	[Theory]
	[AutoData]
	public async Task Position_ShouldChangeWhenReading(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		await That(stream.Position).IsEqualTo(0);
		stream.ReadByte();
		await That(stream.Position).IsEqualTo(1);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task Position_ShouldNotChangeSharedBufferStreamsWhenWriting(
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

		await That(fileStream1.Position).IsEqualTo(initialPosition1 + changedContents.Length);
		await That(fileStream2.Position).IsEqualTo(initialPosition2);

		fileStream1.Flush();

		await That(fileStream1.Position).IsEqualTo(initialPosition1 + changedContents.Length);
		await That(fileStream2.Position).IsEqualTo(initialPosition2);
	}
#endif

	[Theory]
	[AutoData]
	public async Task Seek_Begin_ShouldSetAbsolutePositionFromBegin(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		await That(stream.Position).IsEqualTo(0);
		stream.Seek(4, SeekOrigin.Begin);
		await That(stream.Position).IsEqualTo(4);
	}

	[Theory]
	[AutoData]
	public async Task Seek_Current_ShouldSetRelativePosition(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		await That(stream.Position).IsEqualTo(0);
		stream.Seek(4, SeekOrigin.Current);
		await That(stream.Position).IsEqualTo(4);
		stream.Seek(3, SeekOrigin.Current);
		await That(stream.Position).IsEqualTo(7);
		stream.Seek(-1, SeekOrigin.Current);
		await That(stream.Position).IsEqualTo(6);
	}

	[Theory]
	[AutoData]
	public async Task Seek_End_ShouldSetAbsolutePositionFromEnd(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		await That(stream.Position).IsEqualTo(0);
		stream.Seek(-4, SeekOrigin.End);
		await That(stream.Position).IsEqualTo(contents.Length - 4);
	}

	[Theory]
	[AutoData]
	public async Task SetLength(string path, int length)
	{
		using FileSystemStream stream = FileSystem.File.Create(path);

		stream.SetLength(length);

		await That(stream.Length).IsEqualTo(length);
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

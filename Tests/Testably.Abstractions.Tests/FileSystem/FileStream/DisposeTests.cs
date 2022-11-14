using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DisposeTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Dispose_CalledTwiceShouldDoNothing(
		string path, byte[] contents)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.File.WriteAllBytes(path, contents);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.ReadWrite, 10, FileOptions.DeleteOnClose);

		stream.Dispose();
		FileSystem.File.Exists(path).Should().BeFalse();
		FileSystem.File.WriteAllText(path, "foo");

		stream.Dispose();

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Dispose_ShouldNotResurrectFile(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);
		FileSystemStream stream = FileSystem.File.Open(path,
			FileMode.Open,
			FileAccess.ReadWrite,
			FileShare.Delete);

		int fileCount1 = FileSystem.Directory.GetFiles(".", "*").Length;
		FileSystem.File.Delete(path);
		int fileCount2 = FileSystem.Directory.GetFiles(".", "*").Length;
		stream.Dispose();
		int fileCount3 = FileSystem.Directory.GetFiles(".", "*").Length;

		fileCount1.Should().Be(1, "File should have existed");
		fileCount2.Should().Be(0, "File should have been deleted");
		fileCount3.Should().Be(0, "Dispose should not have resurrected the file");
	}

	[Theory]
	[MemberData(nameof(GetFileStreamCallbacks))]
	public void Operations_ShouldThrowAfterStreamIsDisposed(
		Expression<Action<FileSystemStream>> callback)
	{
		FileSystem.File.WriteAllText("foo", "some content");
		Exception? exception = Record.Exception(() =>
		{
			FileSystemStream stream =
				FileSystem.FileStream.New("foo", FileMode.Open, FileAccess.ReadWrite);
			stream.Dispose();
			callback.Compile().Invoke(stream);
		});

		exception.Should()
			.BeOfType<ObjectDisposedException>(
				$"\n{callback}\n executed after Dispose() was called.")
			.Which.ObjectName.Should()
			.BeEmpty($"\n{callback}\n executed after Dispose() was called.");
	}

	#region Helpers

	public static IEnumerable<object?[]> GetFileStreamCallbacks()
		=> GetFileStreamCallbackTestParameters()
			.Select(item => new object?[]
			{
				item
			});

	private static IEnumerable<Expression<Action<FileSystemStream>>>
		GetFileStreamCallbackTestParameters()
	{
		yield return fileStream => fileStream.BeginRead(Array.Empty<byte>(), 0, 0, null, null);
		yield return fileStream => fileStream.BeginWrite(Array.Empty<byte>(), 0, 0, null, null);
		yield return fileStream => fileStream.CopyTo(new MemoryStream(), 1);
		yield return fileStream
			=> fileStream.CopyToAsync(new MemoryStream(), 1, CancellationToken.None)
				.GetAwaiter().GetResult();
		yield return fileStream => fileStream.Flush();
		yield return fileStream => fileStream.FlushAsync(CancellationToken.None)
			.GetAwaiter().GetResult();
		// ReSharper disable once MustUseReturnValue
		yield return fileStream => fileStream.Read(Array.Empty<byte>(), 0, 0);
#if FEATURE_SPAN
		//yield return fileStream => fileStream.Read(Array.Empty<byte>().AsSpan());
#endif
		yield return fileStream => fileStream.ReadAsync(Array.Empty<byte>(), 0, 0)
			.GetAwaiter().GetResult();
		yield return fileStream => fileStream.ReadByte();
		yield return fileStream => fileStream.Seek(0, SeekOrigin.Begin);
		yield return fileStream => fileStream.SetLength(0);
		yield return fileStream => fileStream.Write(Array.Empty<byte>(), 0, 0);
		yield return fileStream => fileStream.WriteAsync(Array.Empty<byte>(), 0, 0)
			.GetAwaiter().GetResult();
		yield return fileStream => fileStream.WriteByte(0x42);
	}

	#endregion
}

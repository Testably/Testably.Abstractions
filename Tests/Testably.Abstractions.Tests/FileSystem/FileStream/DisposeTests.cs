using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class DisposeTests
{
	[Theory]
	[AutoData]
	public async Task Dispose_CalledTwiceShouldDoNothing(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.ReadWrite, 10, FileOptions.DeleteOnClose);

		// ReSharper disable once DisposeOnUsingVariable
		stream.Dispose();
		await That(FileSystem.File.Exists(path)).IsFalse();
		FileSystem.File.WriteAllText(path, "foo");

		// ReSharper disable once DisposeOnUsingVariable
		stream.Dispose();

		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Dispose_ShouldNotResurrectFile(string path, string contents)
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

		await That(fileCount1).IsEqualTo(1).Because("File should have existed");
		await That(fileCount2).IsEqualTo(0).Because("File should have been deleted");
		await That(fileCount3).IsEqualTo(0).Because("Dispose should not have resurrected the file");
	}

	[Theory]
	[MemberData(nameof(GetFileStreamCallbacks))]
	public async Task Operations_ShouldThrowAfterStreamIsDisposed(
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

		await That(exception).IsExactly<ObjectDisposedException>().Because($"\n{callback}\n executed after Dispose() was called.");
	}

	#region Helpers

#pragma warning disable MA0018
	public static TheoryData<Expression<Action<FileSystemStream>>> GetFileStreamCallbacks()
		=> new(GetFileStreamCallbackTestParameters());
	#pragma warning restore MA0018

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
		#pragma warning disable MA0060
		// ReSharper disable once MustUseReturnValue
		#pragma warning disable CA2022
		yield return fileStream => fileStream.Read(Array.Empty<byte>(), 0, 0);
		#pragma warning restore CA2022
		#pragma warning restore MA0060
		yield return fileStream => fileStream.ReadAsync(Array.Empty<byte>(), 0, 0, TestContext.Current.CancellationToken)
			.GetAwaiter().GetResult();
		yield return fileStream => fileStream.ReadByte();
		yield return fileStream => fileStream.Seek(0, SeekOrigin.Begin);
		yield return fileStream => fileStream.SetLength(0);
		yield return fileStream => fileStream.Write(Array.Empty<byte>(), 0, 0);
		yield return fileStream => fileStream.WriteAsync(Array.Empty<byte>(), 0, 0, TestContext.Current.CancellationToken)
			.GetAwaiter().GetResult();
		yield return fileStream => fileStream.WriteByte(0x42);
	}

	#endregion
}

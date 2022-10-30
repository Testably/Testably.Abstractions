using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileStreamOptionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Options_DeleteOnClose_ShouldDeleteFileOnClose(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.DeleteOnClose);

		stream.Close();

		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Options_DeleteOnClose_ShouldDeleteFileOnDispose(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
			FileAccess.ReadWrite, FileShare.None, 10, FileOptions.DeleteOnClose);

		stream.Dispose();

		FileSystem.File.Exists(path).Should().BeFalse();
	}
}
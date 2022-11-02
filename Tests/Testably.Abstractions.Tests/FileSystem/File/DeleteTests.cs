using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DeleteTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Delete_WithOpenFile_ShouldThrowIOException(string filename)
	{
		FileSystem.Initialize();
		FileSystemStream openFile = FileSystem.File.OpenWrite(filename);
		openFile.Write(new byte[] { 0 }, 0, 1);
		openFile.Flush();
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(filename);
			openFile.Write(new byte[] { 0 }, 0, 1);
			openFile.Flush();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
			   .Which.Message.Should()
			   .Contain($"{filename}'");
			FileSystem.File.Exists(filename).Should().BeTrue();
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.File.Exists(filename).Should().BeFalse();
		}
	}
}
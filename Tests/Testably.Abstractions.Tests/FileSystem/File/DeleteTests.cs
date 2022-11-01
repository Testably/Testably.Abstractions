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
	public void Delete_OpenFile_ShouldThrowIOException(string filename)
	{
		FileSystem.Initialize();
		FileSystemStream openfile = FileSystem.File.OpenWrite(filename);
		openfile.Write(new byte[] { 0 }, 0, 1);
		openfile.Flush();
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(filename);
			openfile.Write(new byte[] { 0 }, 0, 1);
			openfile.Flush();
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(filename)}'");
	}
}
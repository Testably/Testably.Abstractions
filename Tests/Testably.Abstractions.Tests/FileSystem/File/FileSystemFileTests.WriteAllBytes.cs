using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void WriteAllBytes_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, Encoding.UTF8.GetBytes("foo"));

		FileSystem.File.WriteAllBytes(path, contents);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllBytes_ShouldCreateFileWithBytes(string path, byte[] contents)
	{
		FileSystem.File.WriteAllBytes(path, contents);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(contents);
	}
}
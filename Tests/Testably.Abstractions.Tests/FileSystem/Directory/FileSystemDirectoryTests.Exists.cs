namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.Exists))]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Directory.Exists(null);

		result.Should().BeFalse();
	}
}
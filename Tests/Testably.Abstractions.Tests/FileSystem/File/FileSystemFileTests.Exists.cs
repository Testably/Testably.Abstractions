namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(null);

		result.Should().BeFalse();
	}

	[SkippableFact]
	public void Exists_Empty_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(string.Empty);

		result.Should().BeFalse();
	}
}
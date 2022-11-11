#if FEATURE_FILESYSTEM_NET7
namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExistsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.Path.Exists(null);

		result.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ShouldWorkWithAbsolutePaths(string path)
	{
		string absolutePath = FileSystem.Path.GetFullPath(path);
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Path.Exists(absolutePath);

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ExistingFile_ShouldReturnTrue(string path)
	{
		FileSystem.File.WriteAllText(path, "some content");

		bool result = FileSystem.Path.Exists(path);

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ExistingDirectory_ShouldReturnTrue(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.Path.Exists(path);

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ExistingFileOrDirectory_ShouldReturnTrue(string path)
	{
		bool result = FileSystem.Path.Exists(path);

		result.Should().BeFalse();
	}
}
#endif
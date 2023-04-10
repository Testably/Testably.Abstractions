using Testably.Abstractions.Testing.FileSystemInitializer;
#if FEATURE_FILESYSTEM_NET7

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateTempSubdirectoryTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void CreateTempSubdirectory_ShouldCreateTheTemporaryDirectory()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory();

		result.Exists.Should().BeTrue();
		result.FullName.Should().StartWith(FileSystem.Path.GetTempPath());
	}

	[SkippableFact]
	public void CreateTempSubdirectory_ShouldHaveCharactersFromUpperLowerOrDigits()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory();

		string name = FileSystem.Path.GetFileName(result.FullName);
		for (int i = 0; i < name.Length; i++)
		{
			char c = name[i];
			if (!char.IsUpper(c) && !char.IsLower(c) && !char.IsDigit(c))
			{
				throw new TestingException(
					$"Character '{c}' at position {i} of directory {name} is not upper, lower or digit");
			}
		}
	}

	[SkippableFact]
	public void
		CreateTempSubdirectory_WithPrefix_ShouldCreateDirectoryWithGivenPrefixInTempDirectory()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory("foo-");

		FileSystem.Directory.Exists(result.FullName).Should().BeTrue();
		FileSystem.Path.GetFileName(result.FullName).Should().StartWith("foo-");
		result.FullName.Should().StartWith(FileSystem.Path.GetTempPath());
	}

	[SkippableTheory]
	[AutoData]
	public void CreateTempSubdirectory_WithPrefix_ShouldEndWithCharactersFromUpperLowerOfDigits(
		string prefix)
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory(prefix);

		string name = FileSystem.Path.GetFileName(result.FullName);
		for (int i = prefix.Length; i < name.Length; i++)
		{
			char c = name[i];
			if (!char.IsUpper(c) && !char.IsLower(c) && !char.IsDigit(c))
			{
				throw new TestingException(
					$"Character '{c}' at position {i} of directory {name} is not upper, lower or digit");
			}
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CreateTempSubdirectory_WithPrefix_ShouldStartWithPrefix(string prefix)
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory(prefix);

		result.Name.Should().StartWith(prefix);
	}
}
#endif

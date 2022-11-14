using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
#if FEATURE_FILESYSTEM_NET7
	[SkippableFact]
	public void CreateTempSubdirectory_ShouldCreateTheTemporaryDirectory()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory();

		result.Exists.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void CreateTempSubdirectory_WithPrefix_ShouldStartWithPrefix(string prefix)
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory(prefix);

		result.Name.Should().StartWith(prefix);
	}
#endif
	[SkippableFact]
	public void GetCurrentDirectory_ShouldNotBeRooted()
	{
		string result = FileSystem.Directory.GetCurrentDirectory();

		result.Should().NotBe(FileTestHelper.RootDrive());
	}

	[SkippableTheory]
	[AutoData]
	public void GetDirectoryRoot_ShouldReturnRoot(string path)
	{
		string root = FileTestHelper.RootDrive();
		string rootedPath = root + path;

		string result = FileSystem.Directory.GetDirectoryRoot(rootedPath);

		result.Should().Be(root);
	}

	[SkippableFact]
	public void GetLogicalDrives_ShouldNotBeEmpty()
	{
		string[] result = FileSystem.Directory.GetLogicalDrives();

		result.Should().NotBeEmpty();
		result.Should().Contain(FileTestHelper.RootDrive());
	}

	[SkippableTheory]
	[AutoData]
	public void GetParent_ArbitraryPaths_ShouldNotBeNull(string path1,
		string path2,
		string path3)
	{
		string path = FileSystem.Path.Combine(path1, path2, path3);
		IDirectoryInfo expectedParent = FileSystem.DirectoryInfo.New(
			FileSystem.Path.Combine(path1, path2));

		IDirectoryInfo? result = FileSystem.Directory.GetParent(path);

		result.Should().NotBeNull();
		result!.FullName.Should().Be(expectedParent.FullName);
	}

	[SkippableFact]
	public void GetParent_Root_ShouldReturnNull()
	{
		string path = FileTestHelper.RootDrive();

		IDirectoryInfo? result = FileSystem.Directory.GetParent(path);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void
		SetCurrentDirectory_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string previousCurrentDirectory = FileSystem.Directory.GetCurrentDirectory();
		try
		{
			Exception? exception = Record.Exception(() =>
			{
				FileSystem.Directory.SetCurrentDirectory(path);
			});

			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
			FileSystem.Directory.GetCurrentDirectory().Should()
				.Be(previousCurrentDirectory);
		}
		finally
		{
			FileSystem.Directory.SetCurrentDirectory(previousCurrentDirectory);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void SetCurrentDirectory_RelativePath_ShouldBeFullyQualified(string path)
	{
		string previousCurrentDirectory = FileSystem.Directory.GetCurrentDirectory();
		try
		{
			string expectedPath = FileSystem.Path.GetFullPath(path);
			FileSystem.Directory.CreateDirectory(path);
			FileSystem.Directory.SetCurrentDirectory(path);
			string result = FileSystem.Directory.GetCurrentDirectory();

			result.Should().Be(expectedPath);
		}
		finally
		{
			FileSystem.Directory.SetCurrentDirectory(previousCurrentDirectory);
		}
	}
}

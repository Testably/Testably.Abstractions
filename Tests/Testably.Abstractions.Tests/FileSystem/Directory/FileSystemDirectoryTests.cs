using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	protected FileSystemDirectoryTests(TFileSystem fileSystem, ITimeSystem timeSystem)
		: base(fileSystem, timeSystem)
	{
	}

	[SkippableFact]
	public void GetCurrentDirectory_ShouldNotBeRooted()
	{
		string result = FileSystem.Directory.GetCurrentDirectory();

		result.Should().NotBe(FileTestHelper.RootDrive());
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

			exception.Should().BeOfType<DirectoryNotFoundException>();
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
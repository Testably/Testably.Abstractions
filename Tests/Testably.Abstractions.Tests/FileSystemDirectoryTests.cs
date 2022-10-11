namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	#region Test Setup

	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemDirectoryTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	#endregion

	[SkippableFact]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetLogicalDrives))]
	public void GetLogicalDrives_ShouldNotBeEmpty()
	{
		string[] result = FileSystem.Directory.GetLogicalDrives();

		result.Should().NotBeEmpty();
		result.Should().Contain(FileTestHelper.RootDrive());
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetParent))]
	public void GetParent_ArbitraryPaths_ShouldNotBeNull(string path1,
	                                                     string path2,
	                                                     string path3)
	{
		string path = FileSystem.Path.Combine(path1, path2, path3);
		IFileSystem.IDirectoryInfo expectedParent = FileSystem.DirectoryInfo.New(
			FileSystem.Path.Combine(path1, path2));

		IFileSystem.IDirectoryInfo? result = FileSystem.Directory.GetParent(path);

		result.Should().NotBeNull();
		result!.FullName.Should().Be(expectedParent.FullName);
	}
}
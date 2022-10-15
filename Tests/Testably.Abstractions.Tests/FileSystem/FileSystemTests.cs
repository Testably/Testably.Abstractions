namespace Testably.Abstractions.Tests.FileSystem;

public abstract class FileSystemTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	#region Test Setup

	public TFileSystem FileSystem { get; }

	protected FileSystemTests(TFileSystem fileSystem)
	{
		FileSystem = fileSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	#endregion

	[SkippableFact]
	public void Directory_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.Directory.FileSystem;

		result.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void DirectoryInfo_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.DirectoryInfo.FileSystem;

		result.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void DriveInfo_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.DriveInfo.FileSystem;

		result.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void File_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.File.FileSystem;

		result.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void FileInfo_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.FileInfo.FileSystem;

		result.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void FileStream_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.FileStream.FileSystem;

		result.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void FileSystemWatcherFactory_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.FileSystemWatcher.FileSystem;

		result.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void Path_ShouldSetExtensionPoint()
	{
		IFileSystem result = FileSystem.Path.FileSystem;

		result.Should().Be(FileSystem);
	}
}
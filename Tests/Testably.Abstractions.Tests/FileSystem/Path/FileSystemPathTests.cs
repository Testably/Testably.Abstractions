namespace Testably.Abstractions.Tests.FileSystem.Path;

public abstract partial class FileSystemPathTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public TFileSystem FileSystem { get; }

	protected FileSystemPathTests(TFileSystem fileSystem)
	{
		FileSystem = fileSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableFact]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.AltDirectorySeparatorChar))]
	public void AltDirectorySeparatorChar_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.AltDirectorySeparatorChar;

		result.Should().Be(System.IO.Path.AltDirectorySeparatorChar);
	}

	[SkippableFact]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.DirectorySeparatorChar))]
	public void DirectorySeparatorChar_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.DirectorySeparatorChar;

		result.Should().Be(System.IO.Path.DirectorySeparatorChar);
	}

	[SkippableFact]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.PathSeparator))]
	public void PathSeparator_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.PathSeparator;

		result.Should().Be(System.IO.Path.PathSeparator);
	}

	[SkippableFact]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.VolumeSeparatorChar))]
	public void VolumeSeparatorChar_ShouldReturnDefaultValue()
	{
		char result = FileSystem.Path.VolumeSeparatorChar;

		result.Should().Be(System.IO.Path.VolumeSeparatorChar);
	}
}
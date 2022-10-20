using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

public abstract class FileSystemDriveInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemDriveInfoTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableFact]
	public void VolumeLabel_ShouldNotBeWritable()
	{
		IFileSystem.IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive());

		Exception? exception = Record.Exception(() =>
		{
			result.VolumeLabel = "TEST";
		});

		exception.Should().BeOfType<UnauthorizedAccessException>();
	}
}
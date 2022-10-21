using System.Security.AccessControl;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class DirectoryInfoMockTest
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Testing.FileSystemMock fileSystem = new();
		IFileSystem.IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New("foo");

#pragma warning disable CA1416
		DirectorySecurity result =
			directoryInfo.GetAccessControl(AccessControlSections.All);
#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Testing.FileSystemMock fileSystem = new();
		IFileSystem.IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New("foo");
#pragma warning disable CA1416
		DirectorySecurity directorySecurity = new();

		directoryInfo.SetAccessControl(directorySecurity);
		DirectorySecurity result = directoryInfo.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(directorySecurity);
	}

	[SkippableFact]
	public void Create_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Testing.FileSystemMock fileSystem = new();
		IFileSystem.IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New("foo");
#pragma warning disable CA1416
		DirectorySecurity directorySecurity = new();

		directoryInfo.Create(directorySecurity);
		DirectorySecurity result = directoryInfo.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(directorySecurity);
	}
}
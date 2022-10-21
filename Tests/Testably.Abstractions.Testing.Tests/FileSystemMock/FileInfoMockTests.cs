using System.Security.AccessControl;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class FileInfoMockTests
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Testing.FileSystemMock fileSystem = new();
		IFileSystem.IFileInfo fileInfo = fileSystem.FileInfo.New("foo");

		FileSecurity result = fileInfo.GetAccessControl(AccessControlSections.All);

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Testing.FileSystemMock fileSystem = new();
		IFileSystem.IFileInfo fileInfo = fileSystem.FileInfo.New("foo");
		FileSecurity fileSecurity = new();

		fileInfo.SetAccessControl(fileSecurity);
		FileSecurity result = fileInfo.GetAccessControl();

		result.Should().Be(fileSecurity);
	}
}
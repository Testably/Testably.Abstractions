using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

public class FileInfoAclExtensionsTests
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemMock fileSystem = new();
		IFileSystem.IFileInfo fileInfo = fileSystem.FileInfo.New("foo");

#pragma warning disable CA1416
		FileSecurity result = fileInfo.GetAccessControl(AccessControlSections.All);
#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_RealFileSystem_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem fileSystem = new();
		using (fileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory())
		{
			fileSystem.File.WriteAllText("foo", null);
#pragma warning disable CA1416
			FileSecurity originalAccessControl =
				fileSystem.FileInfo.New("foo").GetAccessControl();
			fileSystem.FileInfo.New("foo").SetAccessControl(originalAccessControl);

			FileSecurity currentAccessControl =
				fileSystem.FileInfo.New("foo").GetAccessControl(AccessControlSections.Access);
#pragma warning restore CA1416

			currentAccessControl.HasSameAccessRightsAs(originalAccessControl)
			   .Should().BeTrue();
			currentAccessControl.Should().NotBe(originalAccessControl);
		}
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemMock fileSystem = new();
		IFileSystem.IFileInfo fileInfo = fileSystem.FileInfo.New("foo");
#pragma warning disable CA1416
		FileSecurity fileSecurity = new();

		fileInfo.SetAccessControl(fileSecurity);
		FileSecurity result = fileInfo.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(fileSecurity);
	}
}
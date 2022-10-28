using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

public class FileStreamAclExtensionsTests
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		FileSystemStream fileStream = fileSystem.File.Create("foo");

#pragma warning disable CA1416
		FileSecurity result = fileStream.GetAccessControl();
#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_RealFileSystem_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		RealFileSystem fileSystem = new();
		Test.SkipIfLongRunningTestsShouldBeSkipped(fileSystem);

		using (fileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory())
		{
			FileSystemStream fileStream = fileSystem.File.Create("foo");
#pragma warning disable CA1416
			FileSecurity originalAccessControl = fileStream.GetAccessControl();
			fileStream.SetAccessControl(originalAccessControl);

			FileSecurity currentAccessControl = fileStream.GetAccessControl();
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

		MockFileSystem fileSystem = new();
		FileSystemStream fileStream = fileSystem.File.Create("foo");
#pragma warning disable CA1416
		FileSecurity fileSecurity = new();

		fileStream.SetAccessControl(fileSecurity);
		FileSecurity result = fileStream.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(fileSecurity);
	}
}
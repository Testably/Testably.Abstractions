using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

public class FileAclExtensionsTests
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemMock fileSystem = new();
		fileSystem.File.WriteAllText("foo", null);

#pragma warning disable CA1416
		FileSecurity result =
			fileSystem.File.GetAccessControl("foo", AccessControlSections.All);
#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemMock fileSystem = new();
		fileSystem.File.WriteAllText("foo", null);
#pragma warning disable CA1416
		FileSecurity fileSecurity = new();

		fileSystem.File.SetAccessControl("foo", fileSecurity);
		FileSecurity result = fileSystem.File.GetAccessControl("foo");
#pragma warning restore CA1416

		result.Should().Be(fileSecurity);
	}
}
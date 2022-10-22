using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

public class FileStreamAclExtensionsTests
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemMock fileSystem = new();
		FileSystemStream fileStream = fileSystem.File.Create("foo");

#pragma warning disable CA1416
		FileSecurity result = fileStream.GetAccessControl();
#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemMock fileSystem = new();
		FileSystemStream fileStream = fileSystem.File.Create("foo");
#pragma warning disable CA1416
		FileSecurity fileSecurity = new();

		fileStream.SetAccessControl(fileSecurity);
		FileSecurity result = fileStream.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(fileSecurity);
	}
}
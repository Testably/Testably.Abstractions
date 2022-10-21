using System.Security.AccessControl;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class FileStreamMockTests
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Testing.FileSystemMock fileSystem = new();
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

		Testing.FileSystemMock fileSystem = new();
		FileSystemStream fileStream = fileSystem.File.Create("foo");
#pragma warning disable CA1416
		FileSecurity fileSecurity = new();

		fileStream.SetAccessControl(fileSecurity);
		FileSecurity result = fileStream.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(fileSecurity);
	}
}
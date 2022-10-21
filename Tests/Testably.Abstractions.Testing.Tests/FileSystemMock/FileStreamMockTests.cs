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

		FileSecurity result = fileStream.GetAccessControl();

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Testing.FileSystemMock fileSystem = new();
		FileSystemStream fileStream = fileSystem.File.Create("foo");
		FileSecurity fileSecurity = new();

		fileStream.SetAccessControl(fileSecurity);
		FileSecurity result = fileStream.GetAccessControl();

		result.Should().Be(fileSecurity);
	}
}
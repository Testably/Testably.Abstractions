using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

[FileSystemTests(RequiredOperatingSystem = SimulationMode.Windows)]
public class FileStreamAclExtensionsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		FileSystemStream fileStream = FileSystem.File.Create("foo");

		#pragma warning disable CA1416
		FileSecurity result = fileStream.GetAccessControl();

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Test]
	public async Task SetAccessControl_ShouldChangeAccessControl()
	{
		FileSystemStream fileStream = FileSystem.File.Create("foo");
		#pragma warning disable CA1416
		FileSecurity originalAccessControl = FileSystem.CreateFileSecurity();
		fileStream.SetAccessControl(originalAccessControl);

		FileSecurity currentAccessControl = fileStream.GetAccessControl();
		#pragma warning restore CA1416

		await That(currentAccessControl.HasSameAccessRightsAs(originalAccessControl))
			.IsTrue();
	}
}

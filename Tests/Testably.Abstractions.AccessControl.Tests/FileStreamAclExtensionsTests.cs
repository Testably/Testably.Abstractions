using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

[FileSystemTests]
public partial class FileStreamAclExtensionsTests
{
	[Fact]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemStream fileStream = FileSystem.File.Create("foo");

		#pragma warning disable CA1416
		FileSecurity result = fileStream.GetAccessControl();

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

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

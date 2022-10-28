using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

public class DirectoryInfoAclExtensionsTests
{
	[SkippableFact]
	public void Create_RealFileSystem_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		RealFileSystem fileSystem = new();
		Test.SkipIfLongRunningTestsShouldBeSkipped(fileSystem);

		using (fileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory())
		{
			fileSystem.Directory.CreateDirectory("foo");
#pragma warning disable CA1416
			DirectorySecurity directorySecurity =
				fileSystem.DirectoryInfo.New("foo").GetAccessControl();

			fileSystem.DirectoryInfo.New("bar").Create(directorySecurity);
			DirectorySecurity result =
				fileSystem.DirectoryInfo.New("bar").GetAccessControl();
#pragma warning restore CA1416

			result.HasSameAccessRightsAs(directorySecurity).Should().BeTrue();
			result.Should().NotBe(directorySecurity);
			fileSystem.Directory.Exists("bar").Should().BeTrue();
		}
	}

	[SkippableFact]
	public void Create_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New("foo");
#pragma warning disable CA1416
		DirectorySecurity directorySecurity = new();

		directoryInfo.Create(directorySecurity);
		DirectorySecurity result = directoryInfo.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(directorySecurity);
	}

	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New("foo");

#pragma warning disable CA1416
		DirectorySecurity result =
			directoryInfo.GetAccessControl(AccessControlSections.Access);
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
			fileSystem.Directory.CreateDirectory("foo");
#pragma warning disable CA1416
			DirectorySecurity originalAccessControl =
				fileSystem.DirectoryInfo.New("foo").GetAccessControl();
			fileSystem.DirectoryInfo.New("foo").SetAccessControl(originalAccessControl);

			DirectorySecurity currentAccessControl =
				fileSystem.DirectoryInfo.New("foo").GetAccessControl(AccessControlSections.Access);
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
		IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New("foo");
#pragma warning disable CA1416
		DirectorySecurity directorySecurity = new();

		directoryInfo.SetAccessControl(directorySecurity);
		DirectorySecurity result = directoryInfo.GetAccessControl();
#pragma warning restore CA1416

		result.Should().Be(directorySecurity);
	}
}
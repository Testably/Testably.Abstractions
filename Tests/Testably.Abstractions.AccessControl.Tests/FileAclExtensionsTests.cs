﻿using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

public class FileAclExtensionsTests
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity result =
			fileSystem.File.GetAccessControl("foo", AccessControlSections.All);
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
			fileSystem.File.WriteAllText("foo", null);
			#pragma warning disable CA1416
			FileSecurity originalAccessControl =
				fileSystem.File.GetAccessControl("foo");
			fileSystem.File.SetAccessControl("foo", originalAccessControl);

			FileSecurity currentAccessControl =
				fileSystem.File.GetAccessControl("foo", AccessControlSections.Access);
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
		fileSystem.File.WriteAllText("foo", null);
		#pragma warning disable CA1416
		FileSecurity fileSecurity = new();

		fileSystem.File.SetAccessControl("foo", fileSecurity);
		FileSecurity result = fileSystem.File.GetAccessControl("foo");
		#pragma warning restore CA1416

		result.Should().Be(fileSecurity);
	}
}
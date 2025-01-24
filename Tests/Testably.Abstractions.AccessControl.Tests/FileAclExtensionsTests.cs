using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

[FileSystemTests]
public partial class FileAclExtensionsTests
{
	[Fact]
	public async Task GetAccessControl_MissingFile_ShouldThrowFileNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			_ = FileSystem.File.GetAccessControl("foo");
			#pragma warning restore CA1416
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894);
	}

	[Fact]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity result = FileSystem.File.GetAccessControl("foo");

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task GetAccessControl_ShouldReturnSetResult()
	{
		Skip.IfNot(Test.RunsOnWindows);
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity originalResult = FileSystem.File.GetAccessControl("foo");

		FileSystem.File.SetAccessControl("foo", originalResult);

		FileSecurity result =
			FileSystem.File.GetAccessControl("foo");

		await That(result).Is(originalResult);
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task
		GetAccessControl_WithAccessControlSections_MissingFile_ShouldThrowFileNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			_ = FileSystem.File.GetAccessControl("foo", AccessControlSections.None);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894);
	}

	[Fact]
	public async Task GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity result = FileSystem.File.GetAccessControl("foo", AccessControlSections.None);

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task GetAccessControl_WithAccessControlSections_ShouldReturnSetResult()
	{
		Skip.IfNot(Test.RunsOnWindows);
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity originalResult =
			FileSystem.File.GetAccessControl("foo", AccessControlSections.None);

		FileSystem.File.SetAccessControl("foo", originalResult);

		FileSecurity result =
			FileSystem.File.GetAccessControl("foo", AccessControlSections.None);

		await That(result).Is(originalResult);
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText("foo", null);
		#pragma warning disable CA1416
		FileSecurity originalAccessControl = FileSystem.CreateFileSecurity();
		FileSystem.File.SetAccessControl("foo", originalAccessControl);

		FileSecurity currentAccessControl =
			FileSystem.File.GetAccessControl("foo", AccessControlSections.Access);
		#pragma warning restore CA1416

		await That(currentAccessControl.HasSameAccessRightsAs(originalAccessControl))
			.IsTrue();
	}
}

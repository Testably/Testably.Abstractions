﻿using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileAclExtensionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetAccessControl_MissingFile_ShouldThrowFileNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			_ = FileSystem.File.GetAccessControl("foo");
			#pragma warning restore CA1416
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
	}

	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity result = FileSystem.File.GetAccessControl("foo");
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void GetAccessControl_ShouldReturnSetResult()
	{
		Skip.IfNot(Test.RunsOnWindows);
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity originalResult = FileSystem.File.GetAccessControl("foo");

		FileSystem.File.SetAccessControl("foo", originalResult);

		FileSecurity result =
			FileSystem.File.GetAccessControl("foo");
		#pragma warning restore CA1416

		result.Should().Be(originalResult);
	}

	[SkippableFact]
	public void
		GetAccessControl_WithAccessControlSections_MissingFile_ShouldThrowFileNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			_ = FileSystem.File.GetAccessControl("foo", AccessControlSections.None);
			#pragma warning restore CA1416
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
	}

	[SkippableFact]
	public void GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity result = FileSystem.File.GetAccessControl("foo", AccessControlSections.None);
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void GetAccessControl_WithAccessControlSections_ShouldReturnSetResult()
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
		#pragma warning restore CA1416

		result.Should().Be(originalResult);
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText("foo", null);
		#pragma warning disable CA1416
		FileSecurity originalAccessControl = FileSystem.CreateFileSecurity();
		FileSystem.File.SetAccessControl("foo", originalAccessControl);

		FileSecurity currentAccessControl =
			FileSystem.File.GetAccessControl("foo", AccessControlSections.Access);
		#pragma warning restore CA1416

		currentAccessControl.HasSameAccessRightsAs(originalAccessControl)
			.Should().BeTrue();
	}
}

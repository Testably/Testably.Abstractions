﻿using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileInfoAclExtensionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetAccessControl_MissingFile_ShouldThrowDirectoryNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);
		IFileInfo sut = FileSystem.FileInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			sut.GetAccessControl();
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
		IFileInfo fileInfo = FileSystem.FileInfo.New("foo");

		#pragma warning disable CA1416
		FileSecurity result = fileInfo.GetAccessControl();
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText("foo", null);
		IFileInfo fileInfo = FileSystem.FileInfo.New("foo");

		#pragma warning disable CA1416
		FileSecurity result = fileInfo.GetAccessControl(AccessControlSections.None);
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText("foo", null);
		#pragma warning disable CA1416
		FileSecurity originalAccessControl = FileSystem.CreateFileSecurity();
		FileSystem.FileInfo.New("foo").SetAccessControl(originalAccessControl);

		FileSecurity currentAccessControl =
			FileSystem.FileInfo.New("foo")
				.GetAccessControl(AccessControlSections.Access);
		#pragma warning restore CA1416

		currentAccessControl.HasSameAccessRightsAs(originalAccessControl)
			.Should().BeTrue();
	}
}

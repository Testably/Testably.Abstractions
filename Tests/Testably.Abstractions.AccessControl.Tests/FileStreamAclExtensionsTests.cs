using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileStreamAclExtensionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemStream fileStream = FileSystem.File.Create("foo");

		#pragma warning disable CA1416
		FileSecurity result = fileStream.GetAccessControl();
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystemStream fileStream = FileSystem.File.Create("foo");
		#pragma warning disable CA1416
		FileSecurity originalAccessControl = fileStream.GetAccessControl();
		fileStream.SetAccessControl(originalAccessControl);

		FileSecurity currentAccessControl = fileStream.GetAccessControl();
		#pragma warning restore CA1416

		currentAccessControl.HasSameAccessRightsAs(originalAccessControl)
			.Should().BeTrue();
	}
}

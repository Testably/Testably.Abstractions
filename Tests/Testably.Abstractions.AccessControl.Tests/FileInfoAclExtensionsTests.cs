using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

[FileSystemTests(RequiredOperatingSystem = SimulationMode.Windows)]
public class FileInfoAclExtensionsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task GetAccessControl_MissingFile_ShouldThrowFileNotFoundException()
	{
		IFileInfo sut = FileSystem.FileInfo.New("foo");

		void Act()
		{
			#pragma warning disable CA1416
			_ = sut.GetAccessControl();
			#pragma warning restore CA1416
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894);
	}

	[Test]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		FileSystem.File.WriteAllText("foo", null);
		IFileInfo fileInfo = FileSystem.FileInfo.New("foo");

		#pragma warning disable CA1416
		FileSecurity result = fileInfo.GetAccessControl();

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Test]
	public async Task GetAccessControl_ShouldReturnSetResult()
	{
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity originalResult =
			FileSystem.FileInfo.New("foo").GetAccessControl();

		FileSystem.FileInfo.New("foo").SetAccessControl(originalResult);

		FileSecurity result =
			FileSystem.FileInfo.New("foo").GetAccessControl();

		await That(result).IsEqualTo(originalResult);
		#pragma warning restore CA1416
	}

	[Test]
	public async Task
		GetAccessControl_WithAccessControlSections_MissingFile_ShouldThrowFileNotFoundException()
	{
		IFileInfo sut = FileSystem.FileInfo.New("foo");

		void Act()
		{
			#pragma warning disable CA1416
			_ = sut.GetAccessControl(AccessControlSections.None);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894);
	}

	[Test]
	public async Task GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText("foo", null);
		IFileInfo fileInfo = FileSystem.FileInfo.New("foo");

		#pragma warning disable CA1416
		FileSecurity result = fileInfo.GetAccessControl(AccessControlSections.None);

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Test]
	public async Task GetAccessControl_WithAccessControlSections_ShouldReturnSetResult()
	{
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.File.WriteAllText("foo", null);

		#pragma warning disable CA1416
		FileSecurity originalResult =
			FileSystem.FileInfo.New("foo").GetAccessControl(AccessControlSections.None);

		FileSystem.FileInfo.New("foo").SetAccessControl(originalResult);

		FileSecurity result =
			FileSystem.FileInfo.New("foo").GetAccessControl(AccessControlSections.None);

		await That(result).IsEqualTo(originalResult);
		#pragma warning restore CA1416
	}

	[Test]
	public async Task SetAccessControl_ShouldChangeAccessControl()
	{
		FileSystem.File.WriteAllText("foo", null);
		#pragma warning disable CA1416
		FileSecurity originalAccessControl = FileSystem.CreateFileSecurity();
		FileSystem.FileInfo.New("foo").SetAccessControl(originalAccessControl);

		FileSecurity currentAccessControl =
			FileSystem.FileInfo.New("foo")
				.GetAccessControl(AccessControlSections.Access);
		#pragma warning restore CA1416

		await That(currentAccessControl.HasSameAccessRightsAs(originalAccessControl))
			.IsTrue();
	}
}

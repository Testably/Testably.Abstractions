using AutoFixture.Xunit2;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DirectoryInfoAclExtensionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void Create_NullDirectorySecurity_ShouldThrowArgumentNullException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");
		#pragma warning disable CA1416
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.DirectoryInfo.New("foo").Create(null!);
		});
		#pragma warning restore CA1416

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("directorySecurity");
	}

	[SkippableFact]
	public void Create_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");
		#pragma warning disable CA1416
		DirectorySecurity directorySecurity =
			FileSystemSecurityExtensions.CreateDirectorySecurity();

		FileSystem.DirectoryInfo.New("foo").Create(directorySecurity);
		DirectorySecurity result = FileSystem.Directory.GetAccessControl("foo");
		#pragma warning restore CA1416

		result.HasSameAccessRightsAs(directorySecurity).Should().BeTrue();
		FileSystem.Directory.Exists("foo").Should().BeTrue();
	}

	[SkippableFact]
	public void GetAccessControl_MissingDirectory_ShouldThrowDirectoryNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		Exception? exception = Record.Exception(() =>
		{
			#pragma warning disable CA1416
			sut.GetAccessControl();
			#pragma warning restore CA1416
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
			.Which.HResult.Should().Be(-2147024893);
	}

	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl();
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl(AccessControlSections.None);
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");
		#pragma warning disable CA1416
		DirectorySecurity originalAccessControl =
			FileSystemSecurityExtensions.CreateDirectorySecurity();
		FileSystem.DirectoryInfo.New("foo").SetAccessControl(originalAccessControl);

		DirectorySecurity currentAccessControl =
			FileSystem.Directory.GetAccessControl("foo",
				AccessControlSections.Access);
		#pragma warning restore CA1416

		currentAccessControl.HasSameAccessRightsAs(originalAccessControl)
			.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public async Task SetAccessControl_ShouldNotUpdateTimes(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.Directory.CreateDirectory(path);
		await TimeSystem.Task.Delay(3000);
		DateTime previousCreationTimeUtc = FileSystem.File.GetCreationTimeUtc(path);
		DateTime previousLastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime previousLastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc(path);
		#pragma warning disable CA1416
		FileSystem.DirectoryInfo.New(path).SetAccessControl(new DirectorySecurity());
		#pragma warning restore CA1416

		DateTime creationTimeUtc = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc(path);

		creationTimeUtc.Should().Be(previousCreationTimeUtc);
		lastAccessTimeUtc.Should().Be(previousLastAccessTimeUtc);
		lastWriteTimeUtc.Should().Be(previousLastWriteTimeUtc);
	}
}

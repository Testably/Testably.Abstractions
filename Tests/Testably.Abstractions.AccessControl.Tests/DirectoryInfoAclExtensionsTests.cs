using AutoFixture.Xunit3;
using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

[FileSystemTests]
public partial class DirectoryInfoAclExtensionsTests
{
	[Fact]
	public async Task Create_NullDirectorySecurity_ShouldThrowArgumentNullException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		void Act() => FileSystem.DirectoryInfo.New("foo").Create(null!);
		#pragma warning restore CA1416

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("directorySecurity");
	}

	[Theory]
	[InlineData("foo")]
	[InlineData("foo\\bar")]
	public async Task Create_ShouldChangeAccessControl(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		#pragma warning disable CA1416
		DirectorySecurity directorySecurity = FileSystem.CreateDirectorySecurity();

		FileSystem.DirectoryInfo.New(path).Create(directorySecurity);
		DirectorySecurity result = FileSystem.Directory.GetAccessControl(path);
		#pragma warning restore CA1416

		await That(result.HasSameAccessRightsAs(directorySecurity)).IsTrue();
		await That(FileSystem.Directory.Exists(path)).IsTrue();
	}

	[Fact]
	public async Task GetAccessControl_MissingDirectory_ShouldThrowDirectoryNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		#pragma warning disable CA1416
		void Act() => sut.GetAccessControl();
		#pragma warning restore CA1416

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithHResult(-2147024893);
	}

	[Fact]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl();

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task GetAccessControl_ShouldReturnSetResult()
	{
		Skip.IfNot(Test.RunsOnWindows);
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity originalResult =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl();

		FileSystem.DirectoryInfo.New("foo").SetAccessControl(originalResult);

		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl();

		await That(result).Is(originalResult);
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task
		GetAccessControl_WithAccessControlSections_MissingDirectory_ShouldThrowDirectoryNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		#pragma warning disable CA1416
		void Act() => sut.GetAccessControl(AccessControlSections.None);
		#pragma warning restore CA1416

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithHResult(-2147024893);
	}

	[Fact]
	public async Task
		GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl(AccessControlSections.None);

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task GetAccessControl_WithAccessControlSections_ShouldReturnSetResult()
	{
		Skip.IfNot(Test.RunsOnWindows);
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity originalResult =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl(AccessControlSections.None);

		FileSystem.DirectoryInfo.New("foo").SetAccessControl(originalResult);

		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl(AccessControlSections.None);

		await That(result).Is(originalResult);
		#pragma warning restore CA1416
	}

	[Fact]
	public async Task SetAccessControl_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");
		#pragma warning disable CA1416
		DirectorySecurity originalAccessControl = FileSystem.CreateDirectorySecurity();
		FileSystem.DirectoryInfo.New("foo").SetAccessControl(originalAccessControl);

		DirectorySecurity currentAccessControl =
			FileSystem.Directory.GetAccessControl("foo",
				AccessControlSections.Access);
		#pragma warning restore CA1416

		await That(currentAccessControl.HasSameAccessRightsAs(originalAccessControl))
			.IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task SetAccessControl_ShouldNotUpdateTimes(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Directory.CreateDirectory(path);
		await TimeSystem.Task.Delay(3000, TestContext.Current.CancellationToken);
		DateTime previousCreationTimeUtc = FileSystem.File.GetCreationTimeUtc(path);
		DateTime previousLastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime previousLastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc(path);
		#pragma warning disable CA1416
		FileSystem.DirectoryInfo.New(path).SetAccessControl(new DirectorySecurity());
		#pragma warning restore CA1416

		DateTime creationTimeUtc = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc(path);

		await That(creationTimeUtc).Is(previousCreationTimeUtc);
		await That(lastAccessTimeUtc).Is(previousLastAccessTimeUtc);
		await That(lastWriteTimeUtc).Is(previousLastWriteTimeUtc);
	}
}

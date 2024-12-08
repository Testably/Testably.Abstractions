using AutoFixture.Xunit2;
using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Xunit.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DirectoryInfoAclExtensionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public async Task Create_NullDirectorySecurity_ShouldThrowArgumentNullException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		void Act() => FileSystem.DirectoryInfo.New("foo").Create(null!);
		#pragma warning restore CA1416

		await That(Act).Should().Throw<ArgumentNullException>()
			.WithParamName("directorySecurity");
	}

	[SkippableTheory]
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

		await That(result.HasSameAccessRightsAs(directorySecurity)).Should().BeTrue();
		await That(FileSystem.Directory.Exists(path)).Should().BeTrue();
	}

	[SkippableFact]
	public async Task GetAccessControl_MissingDirectory_ShouldThrowDirectoryNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		#pragma warning disable CA1416
		void Act() => sut.GetAccessControl();
		#pragma warning restore CA1416

		await That(Act).Should().Throw<DirectoryNotFoundException>()
			.WithHResult(-2147024893);
	}

	[SkippableFact]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl();

		await That(result).Should().NotBeNull();
		#pragma warning restore CA1416
	}

	[SkippableFact]
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

		await That(result).Should().Be(originalResult);
		#pragma warning restore CA1416
	}

	[SkippableFact]
	public async Task
		GetAccessControl_WithAccessControlSections_MissingDirectory_ShouldThrowDirectoryNotFoundException()
	{
		Skip.IfNot(Test.RunsOnWindows);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("foo");

		#pragma warning disable CA1416
		void Act() => sut.GetAccessControl(AccessControlSections.None);
		#pragma warning restore CA1416

		await That(Act).Should().Throw<DirectoryNotFoundException>()
			.WithHResult(-2147024893);
	}

	[SkippableFact]
	public async Task
		GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.DirectoryInfo.New("foo").GetAccessControl(AccessControlSections.None);

		await That(result).Should().NotBeNull();
		#pragma warning restore CA1416
	}

	[SkippableFact]
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

		await That(result).Should().Be(originalResult);
		#pragma warning restore CA1416
	}

	[SkippableFact]
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
			.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public async Task SetAccessControl_ShouldNotUpdateTimes(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

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

		await That(creationTimeUtc).Should().Be(previousCreationTimeUtc);
		await That(lastAccessTimeUtc).Should().Be(previousLastAccessTimeUtc);
		await That(lastWriteTimeUtc).Should().Be(previousLastWriteTimeUtc);
	}
}

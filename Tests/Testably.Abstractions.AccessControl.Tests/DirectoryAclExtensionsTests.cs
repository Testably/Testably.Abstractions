using aweXpect;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Xunit.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DirectoryAclExtensionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public async Task CreateDirectory_NullDirectorySecurity_ShouldThrowArgumentNullException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		#pragma warning disable CA1416
		void Act() =>
			FileSystem.Directory.CreateDirectory("foo", null!);
		#pragma warning restore CA1416

		await That(Act).Should().Throw<ArgumentNullException>()
			.WithParamName("directorySecurity");
	}

	[SkippableTheory]
	[InlineData("bar")]
	[InlineData("bar\\foo")]
	public async Task CreateDirectory_ShouldChangeAccessControl(string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		#pragma warning disable CA1416
		DirectorySecurity directorySecurity = FileSystem.CreateDirectorySecurity();

		FileSystem.Directory.CreateDirectory(path, directorySecurity);
		DirectorySecurity result = FileSystem.Directory.GetAccessControl(path);
		#pragma warning restore CA1416

		await That(result.HasSameAccessRightsAs(directorySecurity)).Should().BeTrue();
		await That(FileSystem.Directory.Exists(path)).Should().BeTrue();
	}

	[SkippableFact]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo");

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
			FileSystem.Directory.GetAccessControl("foo");

		FileSystem.Directory.SetAccessControl("foo", originalResult);

		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo");

		await That(result).Should().Be(originalResult);
		#pragma warning restore CA1416
	}

	[SkippableFact]
	public async Task GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo", AccessControlSections.None);

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
			FileSystem.Directory.GetAccessControl("foo", AccessControlSections.None);

		FileSystem.Directory.SetAccessControl("foo", originalResult);

		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo", AccessControlSections.None);

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
		FileSystem.Directory.SetAccessControl("foo", originalAccessControl);

		DirectorySecurity currentAccessControl =
			FileSystem.Directory.GetAccessControl("foo",
				AccessControlSections.Access);
		#pragma warning restore CA1416

		await That(currentAccessControl.HasSameAccessRightsAs(originalAccessControl))
			.Should().BeTrue();
	}

	[SkippableFact]
	public async Task SetAccessControl_ShouldNotUpdateTimes()
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText("foo.txt", "abc");
		await TimeSystem.Task.Delay(3000);
		DateTime previousCreationTimeUtc = FileSystem.File.GetCreationTimeUtc("foo.txt");
		DateTime previousLastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc("foo.txt");
		DateTime previousLastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc("foo.txt");
		#pragma warning disable CA1416
		FileSystem.File.SetAccessControl("foo.txt", new FileSecurity());
		#pragma warning restore CA1416

		DateTime creationTimeUtc = FileSystem.File.GetCreationTimeUtc("foo.txt");
		DateTime lastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc("foo.txt");
		DateTime lastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc("foo.txt");

		await That(creationTimeUtc).Should().Be(previousCreationTimeUtc);
		await That(lastAccessTimeUtc).Should().Be(previousLastAccessTimeUtc);
		await That(lastWriteTimeUtc).Should().Be(previousLastWriteTimeUtc);
	}
}

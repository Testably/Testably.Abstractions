using System.Security.AccessControl;
using System.Threading.Tasks;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;

namespace Testably.Abstractions.AccessControl.Tests;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DirectoryAclExtensionsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void CreateDirectory_NullDirectorySecurity_ShouldThrowArgumentNullException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");
		#pragma warning disable CA1416
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory("foo", null!);
		});
		#pragma warning restore CA1416

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("directorySecurity");
	}

	[SkippableFact]
	public void CreateDirectory_ShouldChangeAccessControl()
	{
		Skip.IfNot(Test.RunsOnWindows);

		#pragma warning disable CA1416
		DirectorySecurity directorySecurity = FileSystem.CreateDirectorySecurity();

		FileSystem.Directory.CreateDirectory("bar", directorySecurity);
		DirectorySecurity result = FileSystem.Directory.GetAccessControl("bar");
		#pragma warning restore CA1416

		result.HasSameAccessRightsAs(directorySecurity).Should().BeTrue();
		FileSystem.Directory.Exists("bar").Should().BeTrue();
	}

	[SkippableFact]
	public void GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo");
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		Skip.IfNot(Test.RunsOnWindows);
		Skip.If(LongRunningTestsShouldBeSkipped());

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo", AccessControlSections.None);
		#pragma warning restore CA1416

		result.Should().NotBeNull();
	}

	[SkippableFact]
	public void SetAccessControl_ShouldChangeAccessControl()
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

		currentAccessControl.HasSameAccessRightsAs(originalAccessControl)
			.Should().BeTrue();
	}

	[SkippableFact]
	public async Task SetAccessControl_ShouldNotUpdateTimes()
	{
		Skip.IfNot(Test.RunsOnWindows);
		Skip.If(LongRunningTestsShouldBeSkipped());

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

		creationTimeUtc.Should().Be(previousCreationTimeUtc);
		lastAccessTimeUtc.Should().Be(previousLastAccessTimeUtc);
		lastWriteTimeUtc.Should().Be(previousLastWriteTimeUtc);
	}
}

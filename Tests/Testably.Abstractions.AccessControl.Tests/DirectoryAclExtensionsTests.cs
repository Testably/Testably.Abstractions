using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Testably.Abstractions.AccessControl.Tests.TestHelpers;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.AccessControl.Tests;

[FileSystemTests(RequiredOperatingSystem = SimulationMode.Windows)]
public class DirectoryAclExtensionsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task CreateDirectory_NullDirectorySecurity_ShouldThrowArgumentNullException()
	{
		#pragma warning disable CA1416
		void Act() =>
			FileSystem.Directory.CreateDirectory("foo", null!);
		#pragma warning restore CA1416

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("directorySecurity");
	}

	[Test]
	[Arguments("bar")]
	[Arguments("bar\\foo")]
	public async Task CreateDirectory_ShouldChangeAccessControl(string path)
	{
		#pragma warning disable CA1416
		DirectorySecurity directorySecurity = FileSystem.CreateDirectorySecurity();

		FileSystem.Directory.CreateDirectory(path, directorySecurity);
		DirectorySecurity result = FileSystem.Directory.GetAccessControl(path);
		#pragma warning restore CA1416

		await That(result.HasSameAccessRightsAs(directorySecurity)).IsTrue();
		await That(FileSystem.Directory.Exists(path)).IsTrue();
	}

	[Test]
	public async Task GetAccessControl_ShouldBeInitializedWithNotNullValue()
	{
		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo");

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Test]
	public async Task GetAccessControl_ShouldReturnSetResult()
	{
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity originalResult =
			FileSystem.Directory.GetAccessControl("foo");

		FileSystem.Directory.SetAccessControl("foo", originalResult);

		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo");

		await That(result).IsEqualTo(originalResult);
		#pragma warning restore CA1416
	}

	[Test]
	public async Task GetAccessControl_WithAccessControlSections_ShouldBeInitializedWithNotNullValue()
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo", AccessControlSections.None);

		await That(result).IsNotNull();
		#pragma warning restore CA1416
	}

	[Test]
	public async Task GetAccessControl_WithAccessControlSections_ShouldReturnSetResult()
	{
		Skip.If(FileSystem is RealFileSystem);

		FileSystem.Directory.CreateDirectory("foo");

		#pragma warning disable CA1416
		DirectorySecurity originalResult =
			FileSystem.Directory.GetAccessControl("foo", AccessControlSections.None);

		FileSystem.Directory.SetAccessControl("foo", originalResult);

		DirectorySecurity result =
			FileSystem.Directory.GetAccessControl("foo", AccessControlSections.None);

		await That(result).IsEqualTo(originalResult);
		#pragma warning restore CA1416
	}

	[Test]
	public async Task SetAccessControl_ShouldChangeAccessControl()
	{
		FileSystem.Directory.CreateDirectory("foo");
		#pragma warning disable CA1416
		DirectorySecurity originalAccessControl = FileSystem.CreateDirectorySecurity();
		FileSystem.Directory.SetAccessControl("foo", originalAccessControl);

		DirectorySecurity currentAccessControl =
			FileSystem.Directory.GetAccessControl("foo",
				AccessControlSections.Access);
		#pragma warning restore CA1416

		await That(currentAccessControl.HasSameAccessRightsAs(originalAccessControl))
			.IsTrue();
	}

	[Test]
	public async Task SetAccessControl_ShouldNotUpdateTimes()
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText("foo.txt", "abc");
		await TimeSystem.Task.Delay(3000, CancellationToken);
		DateTime previousCreationTimeUtc = FileSystem.File.GetCreationTimeUtc("foo.txt");
		DateTime previousLastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc("foo.txt");
		DateTime previousLastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc("foo.txt");
		#pragma warning disable CA1416
		FileSystem.File.SetAccessControl("foo.txt", new FileSecurity());
		#pragma warning restore CA1416

		DateTime creationTimeUtc = FileSystem.File.GetCreationTimeUtc("foo.txt");
		DateTime lastAccessTimeUtc = FileSystem.File.GetLastAccessTimeUtc("foo.txt");
		DateTime lastWriteTimeUtc = FileSystem.File.GetLastWriteTimeUtc("foo.txt");

		await That(creationTimeUtc).IsEqualTo(previousCreationTimeUtc);
		await That(lastAccessTimeUtc).IsEqualTo(previousLastAccessTimeUtc);
		await That(lastWriteTimeUtc).IsEqualTo(previousLastWriteTimeUtc);
	}
}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.Collections.Generic;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class UnixFileModeTests
{
	[Fact]
	public async Task
		CreateDirectory_WhenUnixFileModeOfParentIsNone_ShouldThrowUnauthorizedAccessException()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.Directory.CreateDirectory("parent", UnixFileMode.None);

		void Act()
		{
			FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine("parent", "child"));
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage("Access to the path '*/parent/child' is denied.").AsWildcard();
	}

	[Fact]
	public async Task
		DeleteDirectory_WhenParentUnixFileModeIsNone_ShouldThrowUnauthorizedAccessException()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		IDirectoryInfo fileInfo = FileSystem.Directory.CreateDirectory("parent");
		fileInfo.CreateSubdirectory("child");
		#pragma warning disable CA1416
		fileInfo.UnixFileMode = UnixFileMode.None;
		#pragma warning restore CA1416

		void Act()
		{
			FileSystem.Directory.Delete(FileSystem.Path.Combine("parent", "child"));
		}

		await That(Act).Throws<IOException>()
			.WithHResult(-2146232800).And
			.WithMessage("Access to the path '*/parent/child' is denied.").AsWildcard();
	}

	[Fact]
	public async Task DeleteDirectory_WhenUnixFileModeIsNone_ShouldNotThrow()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.Directory.CreateDirectory("parent", UnixFileMode.None);

		void Act()
		{
			FileSystem.Directory.Delete("parent");
		}

		await That(Act).DoesNotThrow();
		await That(FileSystem.Directory.Exists("parent")).IsFalse();
	}

	[Fact]
	public async Task
		GetDirectories_WhenSubdirectoryHasUnixFileModeSetToNone_ShouldStillIncludeBothDirectories()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.Directory.CreateDirectory("parent");
		FileSystem.Directory.CreateDirectory("parent/foo", UnixFileMode.None);
		FileSystem.Directory.CreateDirectory("parent/bar");

		IEnumerable<string> result = FileSystem.Directory
			.GetDirectories("parent");

		await That(result).HasCount(2);
	}

	[Fact]
	public async Task GetDirectories_WhenUnixFileModeIsNone_ShouldThrowUnauthorizedAccessException()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		IDirectoryInfo baseDirectory =
			FileSystem.Directory.CreateDirectory("parent");
		baseDirectory.CreateSubdirectory("foo");
		baseDirectory.CreateSubdirectory("bar");
		#pragma warning disable CA1416
		baseDirectory.UnixFileMode = UnixFileMode.None;
		#pragma warning restore CA1416

		void Act()
		{
			FileSystem.Directory.GetDirectories("parent");
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage("Access to the path '*/parent' is denied.").AsWildcard();
	}
}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class UnixFileModeTests
{
	[Fact]
	public async Task Create_WhenUnixFileModeOfParentIsNone_ShouldThrowUnauthorizedAccessException()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.Directory.CreateDirectory("parent", UnixFileMode.None);
		IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(FileSystem.Path.Combine("parent", "child"));

		void Act()
		{
			sut.Create();
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage("Access to the path '*/parent/child' is denied.").AsWildcard();
	}

	[Fact]
	public async Task Delete_WhenParentUnixFileModeIsNone_ShouldThrowUnauthorizedAccessException()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		IDirectoryInfo fileInfo = FileSystem.Directory.CreateDirectory("parent");
		IDirectoryInfo sut = fileInfo.CreateSubdirectory("child");
		#pragma warning disable CA1416
		fileInfo.UnixFileMode = UnixFileMode.None;
		#pragma warning restore CA1416

		void Act()
		{
			sut.Delete();
		}

		await That(Act).Throws<IOException>()
			.WithHResult(-2146232800).And
			.WithMessage("Access to the path '*/parent/child' is denied.").AsWildcard();
	}

	[Fact]
	public async Task Delete_WhenUnixFileModeIsNone_ShouldNotThrow()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		IDirectoryInfo sut = FileSystem.Directory.CreateDirectory("parent", UnixFileMode.None);

		void Act()
		{
			sut.Delete();
		}

		await That(Act).DoesNotThrow();
		await That(FileSystem.Directory.Exists("parent")).IsFalse();
	}

	[Fact]
	public async Task
		GetDirectories_WhenSubdirectoryHasUnixFileModeSetToNone_ShouldStillIncludeBothDirectories()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		IDirectoryInfo sut = FileSystem.Directory.CreateDirectory("parent");
		FileSystem.Directory.CreateDirectory("parent/foo", UnixFileMode.None);
		FileSystem.Directory.CreateDirectory("parent/bar");

		IDirectoryInfo[] result = sut.GetDirectories();

		await That(result).HasCount(2);
	}

	[Fact]
	public async Task GetDirectories_WhenUnixFileModeIsNone_ShouldThrowUnauthorizedAccessException()
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		IDirectoryInfo sut =
			FileSystem.Directory.CreateDirectory("parent");
		sut.CreateSubdirectory("foo");
		sut.CreateSubdirectory("bar");
		#pragma warning disable CA1416
		sut.UnixFileMode = UnixFileMode.None;
		#pragma warning restore CA1416

		void Act()
		{
			sut.GetDirectories();
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage("Access to the path '*/parent' is denied.").AsWildcard();
	}
}
#endif

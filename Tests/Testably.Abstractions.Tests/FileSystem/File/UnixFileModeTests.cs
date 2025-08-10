#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class UnixFileModeTests
{
	[Theory]
	[AutoData]
	public async Task GetUnixFileMode_ShouldBeInitializedCorrectly(
		string path)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "");
		UnixFileMode expected = UnixFileMode.OtherRead |
		                        UnixFileMode.GroupRead |
		                        UnixFileMode.UserWrite |
		                        UnixFileMode.UserRead;

		#pragma warning disable CA1416
		UnixFileMode result = FileSystem.File.GetUnixFileMode(path);
		#pragma warning restore CA1416

		await That(result).IsEqualTo(expected);
	}

	[Theory]
	[AutoData]
	public async Task GetUnixFileMode_ShouldThrowPlatformNotSupportedException_OnWindows(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			FileSystem.File.GetUnixFileMode(path);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<PlatformNotSupportedException>().WithHResult(-2146233031);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllText_WhenUnixFileModeIsNone_ShouldThrowUnauthorizedAccessException(
		string path)
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.File.WriteAllText(path, "foo");
		#pragma warning disable CA1416
		FileSystem.File.SetUnixFileMode(path, UnixFileMode.None);
		#pragma warning restore CA1416


		void Act()
		{
			FileSystem.File.ReadAllText(path);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage($"Access to the path '*/{path}' is denied.").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task SetUnixFileMode_MissingFile_ShouldThrowFileNotFoundException(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			FileSystem.File.SetUnixFileMode(path, unixFileMode);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task SetUnixFileMode_ShouldBeSettableOnLinux(
		string path, UnixFileMode unixFileMode)
	{
		Skip.If(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, "some content");

		#pragma warning disable CA1416
		FileSystem.File.SetUnixFileMode(path, unixFileMode);

		UnixFileMode result = FileSystem.File.GetUnixFileMode(path);
		#pragma warning restore CA1416
		await That(result).IsEqualTo(unixFileMode);
	}

	[Theory]
	[AutoData]
	public async Task SetUnixFileMode_ShouldThrowPlatformNotSupportedException_OnWindows(
		string path, UnixFileMode mode)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			#pragma warning disable CA1416
			FileSystem.File.SetUnixFileMode(path, mode);
			#pragma warning restore CA1416
		}

		await That(Act).Throws<PlatformNotSupportedException>().WithHResult(-2146233031);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_WhenUnixFileModeIsNone_ShouldThrowUnauthorizedAccessException(
		string path)
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.File.WriteAllText(path, "foo");
		#pragma warning disable CA1416
		FileSystem.File.SetUnixFileMode(path, UnixFileMode.None);


		void Act()
		{
			FileSystem.File.WriteAllText(path, "bar");
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage($"Access to the path '*/{path}' is denied.").AsWildcard();

		FileSystem.File.SetUnixFileMode(path,
			UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.OtherRead |
			UnixFileMode.OtherWrite);
		#pragma warning restore CA1416
		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo("foo");
	}
}
#endif

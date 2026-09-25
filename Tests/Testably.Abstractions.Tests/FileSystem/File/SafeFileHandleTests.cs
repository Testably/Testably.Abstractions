#if FEATURE_FILESYSTEM_SAFEFILEHANDLE && FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class SafeFileHandleTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	private SafeFileHandle OpenReadWrite(string path)
		=> FileSystem.File.OpenHandle(path, FileMode.Open, FileAccess.ReadWrite);

	[Test]
	[AutoArguments]
	public async Task GetAttributes_ShouldMatchThePathOverload(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetAttributes(handle))
			.IsEqualTo(FileSystem.File.GetAttributes(path));
	}

	[Test]
	[AutoArguments(FileAttributes.ReadOnly)]
	[AutoArguments(FileAttributes.Normal)]
	public async Task SetAttributes_ShouldChangeAttributes(
		FileAttributes attributes, string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetAttributes(handle, attributes);
		}

		await That(FileSystem.File.GetAttributes(path)).IsEqualTo(attributes);
	}

	[Test]
	[AutoArguments]
	public async Task SetAttributes_WithReadOnlyHandle_ShouldThrowUnauthorizedAccessExceptionOnWindows(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Windows requires FILE_WRITE_ATTRIBUTES, which a read-only handle lacks");

		FileSystem.File.WriteAllText(path, contents);
		FileAttributes expectedAttributes = FileSystem.File.GetAttributes(path);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		void Act() => FileSystem.File.SetAttributes(handle, FileAttributes.ReadOnly);

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.File.GetAttributes(path)).IsEqualTo(expectedAttributes);
	}

	[Test]
	[AutoArguments]
	public async Task GetCreationTime_ShouldMatchThePathOverload(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetCreationTime(handle))
			.IsEqualTo(FileSystem.File.GetCreationTime(path));
	}

	[Test]
	[AutoArguments]
	public async Task GetCreationTimeUtc_ShouldMatchThePathOverload(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetCreationTimeUtc(handle))
			.IsEqualTo(FileSystem.File.GetCreationTimeUtc(path));
	}

	[Test]
	[AutoArguments]
	public async Task SetCreationTime_ShouldChangeCreationTime(string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = creationTime.ToLocalTime();
		DateTime expectedTime = creationTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetCreationTime(handle, creationTime);
		}

		await That(FileSystem.File.GetCreationTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Test]
	[AutoArguments]
	public async Task SetCreationTimeUtc_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = creationTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetCreationTimeUtc(handle, creationTime);
		}

		await That(FileSystem.File.GetCreationTimeUtc(path)).IsEqualTo(creationTime);
	}

	[Test]
	[AutoArguments]
	public async Task GetLastAccessTime_ShouldMatchThePathOverload(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetLastAccessTime(handle))
			.IsEqualTo(FileSystem.File.GetLastAccessTime(path));
	}

	[Test]
	[AutoArguments]
	public async Task GetLastAccessTimeUtc_ShouldMatchThePathOverload(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetLastAccessTimeUtc(handle))
			.IsEqualTo(FileSystem.File.GetLastAccessTimeUtc(path));
	}

	[Test]
	[AutoArguments]
	public async Task SetLastAccessTime_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToLocalTime();
		DateTime expectedTime = lastAccessTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetLastAccessTime(handle, lastAccessTime);
		}

		await That(FileSystem.File.GetLastAccessTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Test]
	[AutoArguments]
	public async Task SetLastAccessTimeUtc_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetLastAccessTimeUtc(handle, lastAccessTime);
		}

		await That(FileSystem.File.GetLastAccessTimeUtc(path)).IsEqualTo(lastAccessTime);
	}

	[Test]
	[AutoArguments]
	public async Task GetLastWriteTime_ShouldMatchThePathOverload(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetLastWriteTime(handle))
			.IsEqualTo(FileSystem.File.GetLastWriteTime(path));
	}

	[Test]
	[AutoArguments]
	public async Task GetLastWriteTimeUtc_ShouldMatchThePathOverload(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetLastWriteTimeUtc(handle))
			.IsEqualTo(FileSystem.File.GetLastWriteTimeUtc(path));
	}

	[Test]
	[AutoArguments]
	public async Task SetLastWriteTime_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToLocalTime();
		DateTime expectedTime = lastWriteTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetLastWriteTime(handle, lastWriteTime);
		}

		await That(FileSystem.File.GetLastWriteTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Test]
	[AutoArguments]
	public async Task SetLastWriteTimeUtc_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetLastWriteTimeUtc(handle, lastWriteTime);
		}

		await That(FileSystem.File.GetLastWriteTimeUtc(path)).IsEqualTo(lastWriteTime);
	}

	[Test]
	[AutoArguments]
	public async Task SetTimes_WithReadOnlyHandle_ShouldThrowUnauthorizedAccessExceptionOnWindows(
		string path, DateTime time)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Windows requires FILE_WRITE_ATTRIBUTES, which a read-only handle lacks");

		FileSystem.File.WriteAllText(path, null);
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(() => FileSystem.File.SetCreationTime(handle, time))
			.Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(() => FileSystem.File.SetCreationTimeUtc(handle, time))
			.Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(() => FileSystem.File.SetLastAccessTime(handle, time))
			.Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(() => FileSystem.File.SetLastAccessTimeUtc(handle, time))
			.Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(() => FileSystem.File.SetLastWriteTime(handle, time))
			.Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(() => FileSystem.File.SetLastWriteTimeUtc(handle, time))
			.Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.File.GetCreationTimeUtc(path)).IsEqualTo(creationTime);
		await That(FileSystem.File.GetLastAccessTimeUtc(path)).IsEqualTo(lastAccessTime);
		await That(FileSystem.File.GetLastWriteTimeUtc(path)).IsEqualTo(lastWriteTime);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Test]
	[AutoArguments]
	[UnsupportedOSPlatform("windows")]
	public async Task GetUnixFileMode_ShouldMatchThePathOverload(string path, string contents)
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.File.WriteAllText(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		await That(FileSystem.File.GetUnixFileMode(handle))
			.IsEqualTo(FileSystem.File.GetUnixFileMode(path));
	}

	[Test]
	[AutoArguments]
	[UnsupportedOSPlatform("windows")]
	public async Task SetUnixFileMode_ShouldChangeUnixFileMode(string path, string contents)
	{
		Skip.If(Test.RunsOnWindows, "UnixFileMode is not supported on Windows");

		FileSystem.File.WriteAllText(path, contents);
		const UnixFileMode mode = UnixFileMode.UserRead | UnixFileMode.UserWrite;

		using (SafeFileHandle handle = OpenReadWrite(path))
		{
			FileSystem.File.SetUnixFileMode(handle, mode);
		}

		await That(FileSystem.File.GetUnixFileMode(path)).IsEqualTo(mode);
	}
#endif

}
#endif

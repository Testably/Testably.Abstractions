#if FEATURE_FILE_SPAN
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class AppendAllBytesTests
{
	[Theory]
	[AutoData]
	public async Task AppendAllBytes_ExistingFile_ShouldAppendLinesToFile(
		string path, byte[] previousBytes, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, previousBytes);

		FileSystem.File.AppendAllBytes(path, bytes);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo([..previousBytes, ..bytes]);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, byte[] bytes)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);
		void Act()
		{
			FileSystem.File.AppendAllBytes(filePath, bytes);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_MissingFile_ShouldCreateFile(
		string path, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, bytes);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_ShouldAdjustTimes(string path, byte[] bytes)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.AppendAllBytes(path, bytes);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_Span_ExistingFile_ShouldAppendLinesToFile(
		string path, byte[] previousBytes, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, previousBytes);

		FileSystem.File.AppendAllBytes(path, bytes.AsSpan());

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo([..previousBytes, ..bytes]);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_Span_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, byte[] bytes)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);
		void Act()
		{
			FileSystem.File.AppendAllBytes(filePath, bytes.AsSpan());
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_Span_MissingFile_ShouldCreateFile(
		string path, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, bytes.AsSpan());

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_Span_ShouldAdjustTimes(string path, byte[] bytes)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.AppendAllBytes(path, bytes.AsSpan());

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllBytes_Span_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.File.AppendAllBytes(path, Array.Empty<byte>().AsSpan());
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_Span_WhenFileIsHidden_ShouldNotThrowException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		void Act()
		{
			FileSystem.File.AppendAllBytes(path, bytes.AsSpan());
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllBytes_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.File.AppendAllBytes(path, Array.Empty<byte>());
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_WhenFileIsHidden_ShouldNotThrowException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		void Act()
		{
			FileSystem.File.AppendAllBytes(path, bytes);
		}

		await That(Act).DoesNotThrow();
	}
}
#endif

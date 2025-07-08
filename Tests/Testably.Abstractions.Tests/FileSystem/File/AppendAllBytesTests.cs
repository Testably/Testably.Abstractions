#if FEATURE_FILE_SPAN
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class AppendAllBytesTests
{
	[Theory]
	[AutoData]
	public void AppendAllBytes_ExistingFile_ShouldAppendLinesToFile(
		string path, byte[] previousBytes, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, previousBytes);

		FileSystem.File.AppendAllBytes(path, bytes);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo([..previousBytes, ..bytes]);
	}

	[Theory]
	[AutoData]
	public void AppendAllBytes_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, byte[] bytes)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllBytes(filePath, bytes);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public void AppendAllBytes_MissingFile_ShouldCreateFile(
		string path, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, bytes);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
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
			creationTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public void AppendAllBytes_Span_ExistingFile_ShouldAppendLinesToFile(
		string path, byte[] previousBytes, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, previousBytes);

		FileSystem.File.AppendAllBytes(path, bytes.AsSpan());

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo([..previousBytes, ..bytes]);
	}

	[Theory]
	[AutoData]
	public void AppendAllBytes_Span_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, byte[] bytes)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllBytes(filePath, bytes.AsSpan());
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public void AppendAllBytes_Span_MissingFile_ShouldCreateFile(
		string path, byte[] bytes)
	{
		FileSystem.File.AppendAllBytes(path, bytes.AsSpan());

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(bytes);
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
			creationTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public void
		AppendAllBytes_Span_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllBytes(path, Array.Empty<byte>().AsSpan());
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_Span_WhenFileIsHidden_ShouldNotThrowException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllBytes(path, bytes.AsSpan());
		});

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public void
		AppendAllBytes_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllBytes(path, Array.Empty<byte>());
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytes_WhenFileIsHidden_ShouldNotThrowException(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllBytes(path, bytes);
		});

		await That(exception).IsNull();
	}
}
#endif

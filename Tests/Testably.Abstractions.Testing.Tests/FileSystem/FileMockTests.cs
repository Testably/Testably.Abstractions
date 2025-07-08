#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
using Microsoft.Win32.SafeHandles;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#endif

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class FileMockTests
{
#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task GetAttributes_SafeFileHandle_WithMissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		Exception? exception = Record.Exception(() =>
		{
			_ = fileSystem.File.GetAttributes(fileHandle);
		});

		await That(exception).IsExactly<FileNotFoundException>();
	}
#endif
#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task GetUnixFileMode_SafeFileHandle_ShouldThrowPlatformNotSupportedExceptionOnWindows(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows);

		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		Exception? exception = Record.Exception(() =>
		{
#pragma warning disable CA1416
			fileSystem.File.GetUnixFileMode(fileHandle);
#pragma warning restore CA1416
		});

		await That(exception).IsExactly<PlatformNotSupportedException>();
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetAttributes_SafeFileHandle_ShouldUpdateValue(
		string path, FileAttributes attributes)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText("foo", null);
		fileSystem.File.SetAttributes("foo", attributes);
		FileAttributes expectedAttributes = fileSystem.File.GetAttributes("foo");
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetAttributes(fileHandle, attributes);

		FileAttributes result = fileSystem.File.GetAttributes(fileHandle);
		await That(result).IsEqualTo(expectedAttributes);
	}
#endif

	[Theory]
	[AutoData]
	public async Task SetCreationTime(string path, DateTime creationTime)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTime(path, creationTime);

		await That(fileSystem.File.GetCreationTime(path)).IsEqualTo(creationTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetCreationTime_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime creationTime)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetCreationTime(fileHandle, creationTime);

		DateTime result = fileSystem.File.GetCreationTime(fileHandle);

		await That(result).IsEqualTo(creationTime);
	}
#endif

	[Theory]
	[AutoData]
	public async Task SetCreationTimeUtc(string path, DateTime creationTime)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTimeUtc(path, creationTime);

		await That(fileSystem.File.GetCreationTimeUtc(path)).IsEqualTo(creationTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetCreationTimeUtc_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime creationTimeUtc)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetCreationTimeUtc(fileHandle, creationTimeUtc);

		DateTime result = fileSystem.File.GetCreationTimeUtc(fileHandle);

		await That(result).IsEqualTo(creationTimeUtc);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetLastAccessTime_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastAccessTime)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastAccessTime(fileHandle, lastAccessTime);

		DateTime result = fileSystem.File.GetLastAccessTime(fileHandle);

		await That(result).IsEqualTo(lastAccessTime);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetLastAccessTimeUtc_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastAccessTimeUtc)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastAccessTimeUtc(fileHandle, lastAccessTimeUtc);

		DateTime result = fileSystem.File.GetLastAccessTimeUtc(fileHandle);

		await That(result).IsEqualTo(lastAccessTimeUtc);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetLastWriteTime_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastWriteTime)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastWriteTime(fileHandle, lastWriteTime);

		DateTime result = fileSystem.File.GetLastWriteTime(fileHandle);

		await That(result).IsEqualTo(lastWriteTime);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetLastWriteTimeUtc_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastWriteTimeUtc)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastWriteTimeUtc(fileHandle, lastWriteTimeUtc);

		DateTime result = fileSystem.File.GetLastWriteTimeUtc(fileHandle);

		await That(result).IsEqualTo(lastWriteTimeUtc);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public async Task SetUnixFileMode_SafeFileHandle_ShouldUpdateValue(
		string path, UnixFileMode mode)
	{
		Skip.If(Test.RunsOnWindows);

		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

#pragma warning disable CA1416
		fileSystem.File.SetUnixFileMode(fileHandle, mode);

		UnixFileMode result = fileSystem.File.GetUnixFileMode(fileHandle);
#pragma warning restore CA1416

		await That(result).IsEqualTo(mode);
	}
#endif
}

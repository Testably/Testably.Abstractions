#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
using Microsoft.Win32.SafeHandles;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#endif

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class FileMockTests
{
	[Theory]
	[AutoData]
	public void SetCreationTime(string path, DateTime creationTime)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTime(path, creationTime);

		fileSystem.File.GetCreationTime(path).Should().Be(creationTime);
	}

	[Theory]
	[AutoData]
	public void SetCreationTimeUtc(string path, DateTime creationTime)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTimeUtc(path, creationTime);

		fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTime);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public void SetAttributes_SafeFileHandle_ShouldUpdateValue(
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
		result.Should().Be(expectedAttributes);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public void SetCreationTime_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime creationTime)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetCreationTime(fileHandle, creationTime);

		DateTime result = fileSystem.File.GetCreationTime(fileHandle);

		result.Should().Be(creationTime);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public void SetCreationTimeUtc_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime creationTimeUtc)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetCreationTimeUtc(fileHandle, creationTimeUtc);

		DateTime result = fileSystem.File.GetCreationTimeUtc(fileHandle);

		result.Should().Be(creationTimeUtc);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public void SetLastAccessTime_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastAccessTime)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastAccessTime(fileHandle, lastAccessTime);

		DateTime result = fileSystem.File.GetLastAccessTime(fileHandle);

		result.Should().Be(lastAccessTime);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public void SetLastAccessTimeUtc_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastAccessTimeUtc)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastAccessTimeUtc(fileHandle, lastAccessTimeUtc);

		DateTime result = fileSystem.File.GetLastAccessTimeUtc(fileHandle);

		result.Should().Be(lastAccessTimeUtc);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public void SetLastWriteTime_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastWriteTime)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastWriteTime(fileHandle, lastWriteTime);

		DateTime result = fileSystem.File.GetLastWriteTime(fileHandle);

		result.Should().Be(lastWriteTime);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[Theory]
	[AutoData]
	public void SetLastWriteTimeUtc_SafeFileHandle_ShouldUpdateValue(
		string path, DateTime lastWriteTimeUtc)
	{
		SafeFileHandle fileHandle = new();
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");
		fileSystem.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		fileSystem.File.SetLastWriteTimeUtc(fileHandle, lastWriteTimeUtc);

		DateTime result = fileSystem.File.GetLastWriteTimeUtc(fileHandle);

		result.Should().Be(lastWriteTimeUtc);
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	[SkippableTheory]
	[AutoData]
	public void SetUnixFileMode_SafeFileHandle_ShouldUpdateValue(
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

		result.Should().Be(mode);
	}
#endif
}

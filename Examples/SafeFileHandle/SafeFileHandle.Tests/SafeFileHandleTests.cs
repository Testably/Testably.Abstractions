using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using Testably.Abstractions;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.FileSystem;
using Xunit;

namespace SafeFileHandle.Tests;

public class SafeFileHandleTests
{
	[SkippableTheory]
	[AutoData]
	public void SynchronizeLastAccessTimeFromRealFileSystem_WhenUsingACustomSafeFileHandleStrategy(
		DateTime lastAccessTime, string realFileSystemPath, string mockFileSystemPath)
	{
		Skip.If(true, "Github actions don't support testing SafeFileHandle.");

		// Setup
		RealFileSystem realFileSystem = new();
		MockFileSystem mockFileSystem = new();
		CustomSafeFileHandleStrategy safeFileHandleStrategy = new(realFileSystem, mockFileSystem);
		mockFileSystem.WithSafeFileHandleStrategy(safeFileHandleStrategy);
		using (realFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory())
		{
			// Create correct files on real and mock file system:
			mockFileSystem.File.WriteAllText(mockFileSystemPath, "some content");
			realFileSystem.File.WriteAllText(realFileSystemPath, "some content");
			// Set the last access time on the real file system:
			realFileSystem.File.SetLastAccessTime(realFileSystemPath, lastAccessTime);
			Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle =
				UnmanagedFileLoader.CreateSafeFileHandle(realFileSystemPath);
			// Ensure, that the mock file system last access time is currently different:
			mockFileSystem.File.GetLastAccessTime(mockFileSystemPath)
				.Should().NotBe(lastAccessTime);

			// Add the mapping in the custom ISafeFileHandleStrategy:
			safeFileHandleStrategy.AddMapping(fileHandle, realFileSystemPath,
				new SafeFileHandleMock(mockFileSystemPath));

			// Ensure, that access via the SafeFileHandle is possible
			mockFileSystem.File.GetLastAccessTime(fileHandle)
				.Should().Be(lastAccessTime);
			// Ensure that the last access time was synchronized
			mockFileSystem.File.GetLastAccessTime(mockFileSystemPath)
				.Should().Be(lastAccessTime);
		}
	}
}

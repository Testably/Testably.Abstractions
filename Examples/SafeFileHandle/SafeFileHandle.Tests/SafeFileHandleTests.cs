using aweXpect;
using System;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.FileSystem;
using Xunit;

namespace Testably.Abstractions.Examples.SafeFileHandle.Tests;

public class SafeFileHandleTests
{
	[Theory]
	[InlineData("real", "mock")]
	public async Task
		SynchronizeLastAccessTimeFromRealFileSystem_WhenUsingACustomSafeFileHandleStrategy(
			string realFileSystemPath, string mockFileSystemPath)
	{
		DateTime lastAccessTime = DateTime.Now;
		Skip.Test("Github actions don't support testing SafeFileHandle.");

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
			await Expect.That(mockFileSystem.File.GetLastAccessTime(mockFileSystemPath))
				.IsNotEqualTo(lastAccessTime);

			// Add the mapping in the custom ISafeFileHandleStrategy:
			safeFileHandleStrategy.AddMapping(fileHandle, realFileSystemPath,
				new SafeFileHandleMock(mockFileSystemPath));

			// Ensure, that access via the SafeFileHandle is possible
			await Expect.That(mockFileSystem.File.GetLastAccessTime(fileHandle))
				.IsEqualTo(lastAccessTime);
			// Ensure that the last access time was synchronized
			await Expect.That(mockFileSystem.File.GetLastAccessTime(mockFileSystemPath))
				.IsEqualTo(lastAccessTime);
		}
	}
}

using System.Collections.Generic;
using Testably.Abstractions;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.FileSystem;

namespace SafeFileHandle;

/// <summary>
///     A custom <see cref="ISafeFileHandleStrategy" />, that keeps a dictionary of all mappings and on each access,
///     synchronizes the<br />
///     - <see cref="IFileInfo.Attributes" /><br />
///     - <see cref="IFileInfo.CreationTime" /><br />
///     - <see cref="IFileInfo.LastAccessTime" /><br />
///     - <see cref="IFileInfo.LastWriteTime" /><br />
///     from the real file system to the mock file system.
/// </summary>
public class CustomSafeFileHandleStrategy : ISafeFileHandleStrategy
{
	private readonly RealFileSystem _realFileSystem;
	private readonly MockFileSystem _mockFileSystem;

	private readonly Dictionary<Microsoft.Win32.SafeHandles.SafeFileHandle, (string RealPath,
			SafeFileHandleMock Mock)>
		_mapping = new();

	public CustomSafeFileHandleStrategy(RealFileSystem realFileSystem,
		MockFileSystem mockFileSystem)
	{
		_realFileSystem = realFileSystem;
		_mockFileSystem = mockFileSystem;
	}

	/// <inheritdoc cref="ISafeFileHandleStrategy.MapSafeFileHandle(Microsoft.Win32.SafeHandles.SafeFileHandle)" />
	public SafeFileHandleMock MapSafeFileHandle(
		Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle)
	{
		if (_mapping.TryGetValue(fileHandle, out (string RealPath, SafeFileHandleMock Mock) mock))
		{
			SynchronizeFile(mock.RealPath, mock.Mock.Path);
			return mock.Mock;
		}

		throw new KeyNotFoundException("The provided fileHandle is not mapped!");
	}

	private void SynchronizeFile(string realPath, string mockPath)
	{
		_mockFileSystem.File.SetAttributes(mockPath,
			_realFileSystem.File.GetAttributes(realPath));
		_mockFileSystem.File.SetCreationTime(mockPath,
			_realFileSystem.File.GetCreationTime(realPath));
		_mockFileSystem.File.SetLastAccessTime(mockPath,
			_realFileSystem.File.GetLastAccessTime(realPath));
		_mockFileSystem.File.SetLastWriteTime(mockPath,
			_realFileSystem.File.GetLastWriteTime(realPath));
	}

	/// <summary>
	///     Adds a new mapping for the <see cref="CustomSafeFileHandleStrategy" />.
	/// </summary>
	/// <param name="fileHandle">The safe file handle.</param>
	/// <param name="realFilePath">The path to the file on the real file system.</param>
	/// <param name="fileHandleMock">The safe file handle mock.</param>
	public CustomSafeFileHandleStrategy AddMapping(
		Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle,
		string realFilePath,
		SafeFileHandleMock fileHandleMock)
	{
		_mapping[fileHandle] = (realFilePath, fileHandleMock);
		return this;
	}
}

using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Helpers;

internal static class FileModeHelper
{
	/// <summary>
	///     Returns the file container to open at <paramref name="location" /> in <paramref name="mode" />, creating it
	///     when the mode allows, and throws what the real file system throws when it cannot be opened.
	/// </summary>
	internal static IStorageContainer GetFileContainer(MockFileSystem fileSystem,
		IStorageLocation location,
		FileMode mode,
		FileAccess access,
		IFileSystemExtensibility? extensibility = null)
	{
		location.ThrowExceptionIfNotFound(fileSystem, true);
		IStorageContainer container = fileSystem.Storage.GetContainer(location);
		if (container is NullContainer)
		{
			if (mode is FileMode.Open or FileMode.Truncate)
			{
				throw ExceptionFactory.FileNotFound(location.FullPath);
			}

			container = fileSystem.Storage.GetOrCreateContainer(location,
				InMemoryContainer.NewFile,
				extensibility);
		}
		else if (container.Type == FileSystemTypes.Directory)
		{
			if (fileSystem.Execute.IsWindows)
			{
				throw ExceptionFactory.AccessToPathDenied(location.FullPath);
			}

			throw ExceptionFactory.FileAlreadyExists(location.FullPath, 17);
		}
		else if (mode == FileMode.CreateNew)
		{
			throw ExceptionFactory.FileAlreadyExists(location.FullPath,
				fileSystem.Execute.IsWindows ? -2147024816 : 17);
		}

		if (container.Attributes.HasFlag(FileAttributes.ReadOnly) &&
		    access.HasFlag(FileAccess.Write))
		{
			throw ExceptionFactory.AccessToPathDenied(location.FullPath);
		}

		return container;
	}

	internal static void ThrowIfInvalidModeAccess(FileMode mode, FileAccess access)
	{
		if (mode == FileMode.Append)
		{
			if (access == FileAccess.Read)
			{
				throw ExceptionFactory.InvalidAccessCombination(mode, access);
			}

			if (access != FileAccess.Write)
			{
				throw ExceptionFactory.AppendAccessOnlyInWriteOnlyMode();
			}
		}

		if (!access.HasFlag(FileAccess.Write) &&
		    (mode == FileMode.Truncate || mode == FileMode.CreateNew ||
		     mode == FileMode.Create || mode == FileMode.Append))
		{
			throw ExceptionFactory.InvalidAccessCombination(mode, access);
		}
	}
}

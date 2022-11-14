using System;
using System.Diagnostics.CodeAnalysis;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Storage;

internal static class LocationExtensions
{
	[return: NotNullIfNotNull("location")]
	public static IStorageLocation? ThrowIfNotFound(
		this IStorageLocation? location, MockFileSystem fileSystem,
		Action fileNotFoundException,
		Action? directoryNotFoundException = null)
	{
		if (location == null)
		{
			fileNotFoundException();
		}

		if (fileSystem.Storage.GetContainer(location) is NullContainer)
		{
			IStorageLocation? parentLocation = location?.GetParent();
			if (directoryNotFoundException != null &&
			    parentLocation != null &&
			    fileSystem.Path.GetPathRoot(parentLocation.FullPath) !=
			    parentLocation.FullPath &&
			    fileSystem.Storage.GetContainer(parentLocation) is NullContainer)
			{
				directoryNotFoundException.Invoke();
			}
			else
			{
				fileNotFoundException();
			}
		}

		return location;
	}

	public static IStorageLocation ThrowExceptionIfNotFound(
		this IStorageLocation location, MockFileSystem fileSystem,
		bool allowMissingFile = false,
		Func<string, Exception>? onDirectoryNotFound = null,
		Func<string, Exception>? onFileNotFound = null)
	{
		if (fileSystem.Storage.GetContainer(location) is NullContainer)
		{
			IStorageLocation? parentLocation = location.GetParent();
			if (parentLocation != null &&
			    fileSystem.Path.GetPathRoot(parentLocation.FullPath) !=
			    parentLocation.FullPath &&
			    fileSystem.Storage.GetContainer(parentLocation) is NullContainer)
			{
				throw onDirectoryNotFound?.Invoke(location.FullPath)
				      ?? ExceptionFactory.DirectoryNotFound(location.FullPath);
			}

			if (!allowMissingFile)
			{
				throw onFileNotFound?.Invoke(location.FullPath)
				      ?? ExceptionFactory.FileNotFound(location.FullPath);
			}
		}

		return location;
	}
}

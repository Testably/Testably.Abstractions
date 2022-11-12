using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Testably.Abstractions.Testing.FileSystemInitializer;
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
}
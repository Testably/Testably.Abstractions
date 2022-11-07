﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Helpers;

internal static class FileSystemExtensions
{
	/// <summary>
	///     Determines the new <see cref="IStorageLocation" /> when the <paramref name="location" /> is moved
	///     from <paramref name="source" /> to <paramref name="destination" />.
	/// </summary>
	/// <param name="fileSystem">The <see cref="MockFileSystem" />.</param>
	/// <param name="location">The original location of the file or directory.</param>
	/// <param name="source">The source of the move request.</param>
	/// <param name="destination">The destination of the move request.</param>
	/// <returns>The new <see cref="IStorageLocation" /> under <paramref name="destination" />.</returns>
	internal static IStorageLocation GetMoveLocation(this MockFileSystem fileSystem,
	                                                 IStorageLocation location,
	                                                 IStorageLocation source,
	                                                 IStorageLocation destination)
	{
		if (!location.FullPath.StartsWith(source.FullPath))
		{
			throw new NotSupportedException(
				$"The location '{location.FullPath}' is not under source '{source.FullPath}'!");
		}

		string destinationPath =
			$"{destination.FullPath}{location.FullPath.Substring(source.FullPath.Length)}";
		IStorageLocation destinationLocation =
			fileSystem.Storage.GetLocation(destinationPath, location.FriendlyName);
		return destinationLocation;
	}

	/// <summary>
	///     Returns the relative subdirectory path from <paramref name="fullFilePath" /> to the <paramref name="givenPath" />.
	/// </summary>
	internal static string GetSubdirectoryPath(this IFileSystem fileSystem,
	                                           string fullFilePath,
	                                           string givenPath)
	{
		if (fileSystem.Path.IsPathRooted(givenPath))
		{
			return fullFilePath;
		}

		string currentDirectory = fileSystem.Directory.GetCurrentDirectory();
		if (currentDirectory == string.Empty.PrefixRoot())
		{
			fullFilePath = fullFilePath.Substring(currentDirectory.Length);
		}
		else if (fullFilePath.StartsWith(currentDirectory + Path.DirectorySeparatorChar))
		{
			fullFilePath = fullFilePath.Substring(currentDirectory.Length + 1);
		}
		else
		{
			List<string> parentDirectories = new();
			string? parentName = currentDirectory;
			while (parentName != null &&
			       !fullFilePath.StartsWith(parentName + Path.DirectorySeparatorChar))
			{
				parentDirectories.Add(Path.GetFileName(parentName));
				parentName = Path.GetDirectoryName(parentName);
			}

			if (parentName != null)
			{
				fullFilePath = fullFilePath.Substring(parentName.Length + 1);
			}

			fullFilePath = Path.Combine(
				Path.Combine(parentDirectories.Select(_ => "..").ToArray()),
				Path.Combine(parentDirectories.ToArray()),
				fullFilePath);
		}

		if (!fullFilePath.StartsWith(givenPath + fileSystem.Path.DirectorySeparatorChar))
		{
			return fileSystem.Path.Combine(givenPath, fullFilePath);
		}

		return fullFilePath;
	}
}
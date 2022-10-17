using System;
using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Internal;

internal static class FileSystemExtensions
{
	/// <summary>
	///     Determines the new <see cref="IStorageLocation" /> when the <paramref name="location" /> is moved
	///     from <paramref name="source" /> to <paramref name="destination" />.
	/// </summary>
	/// <param name="fileSystem">The <see cref="FileSystemMock" />.</param>
	/// <param name="location">The original location of the file or directory.</param>
	/// <param name="source">The source of the move request.</param>
	/// <param name="destination">The destination of the move request.</param>
	/// <returns>The new <see cref="IStorageLocation" /> under <paramref name="destination" />.</returns>
	internal static IStorageLocation GetMoveLocation(this FileSystemMock fileSystem,
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
	///     <br />
	///     If <paramref name="recurseSubdirectories" /> is <see langword="false" /> and the <paramref name="givenPath" /> is
	///     not rooted, the result is prefixed with the <paramref name="givenPath" />.
	/// </summary>
	internal static string GetSubdirectoryPath(this IFileSystem fileSystem,
	                                           string fullFilePath,
	                                           string givenPath,
	                                           bool recurseSubdirectories)
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
		else
		{
			fullFilePath = fullFilePath.Substring(currentDirectory.Length + 1);
		}

		if (!recurseSubdirectories && !fullFilePath.StartsWith(givenPath))
		{
			return Path.Combine(givenPath, fullFilePath);
		}

		return fullFilePath;
	}
}
using System;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Helpers;

internal static class FileSystemExtensions
{
	/// <summary>
	///     Returns the <see cref="Execute" /> from the <paramref name="fileSystem" />, if it is a
	///     <see cref="MockFileSystem" />, otherwise a default <see cref="Execute" />.
	/// </summary>
	internal static Execute ExecuteOrDefault(this IFileSystem fileSystem)
	{
		if (fileSystem is MockFileSystem mockFileSystem)
		{
			return mockFileSystem.Execute;
		}

		return new Execute(new MockFileSystem());
	}

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
		if (!location.FullPath.StartsWith(source.FullPath, fileSystem.Execute.StringComparisonMode))
		{
			throw new NotSupportedException(
				$"The location '{location.FullPath}' is not under source '{source.FullPath}'!");
		}

		string destinationPath =
			$"{destination.FullPath}{location.FullPath.Substring(source.FullPath.Length)}";
		// Both parts are already normalized full paths, so resolving the combination again would not change it,
		// unless the destination ends with a separator (e.g. a root) or is on a network share.
		if (!destination.FullPath.EndsWith(fileSystem.Execute.Path.DirectorySeparatorChar) &&
		    !destinationPath.IsUncPath(fileSystem))
		{
			// `GetLocation` also uses the path as the friendly name.
			return InMemoryLocation.New(fileSystem, destination.Drive, destinationPath, destinationPath);
		}

		IStorageLocation destinationLocation =
			fileSystem.Storage.GetLocation(destinationPath, location.FriendlyName);
		return destinationLocation;
	}

	/// <summary>
	///     Creates a function that returns the relative subdirectory path from a full file path and its friendly name to
	///     the <paramref name="givenPath" />.
	/// </summary>
	/// <remarks>
	///     The parts that only depend on the <paramref name="givenPath" /> are computed once, because the function is
	///     called for every enumerated item.
	/// </remarks>
	internal static Func<string, string, string> CreateSubdirectoryPathResolver(
		this MockFileSystem fileSystem,
		string givenPath)
	{
		if (fileSystem.Execute.Path.IsPathRooted(givenPath))
		{
			bool isNtDeviceAlias = givenPath.IsNtDeviceAlias(fileSystem.Execute);
			return (fullFilePath, friendlyName) =>
			{
				if (friendlyName.Equals("/.", StringComparison.Ordinal) ||
				    friendlyName.Equals("/..", StringComparison.Ordinal))
				{
					return friendlyName;
				}

				if (isNtDeviceAlias)
				{
					#pragma warning disable CA1845
					return givenPath.Substring(0, 4) + fullFilePath;
					#pragma warning restore CA1845
				}

				return fullFilePath;
			};
		}

		string currentDirectory = fileSystem.Execute.Path.GetFullPath(givenPath);
		bool isRoot = string.Equals(currentDirectory, string.Empty.PrefixRoot(fileSystem),
			fileSystem.Execute.StringComparisonMode);
		string currentDirectoryWithSeparator =
			currentDirectory + fileSystem.Execute.Path.DirectorySeparatorChar;
		return (fullFilePath, friendlyName) => GetSubdirectoryPath(fileSystem, fullFilePath,
			friendlyName, givenPath, currentDirectory, isRoot, currentDirectoryWithSeparator);
	}

	private static string GetSubdirectoryPath(MockFileSystem fileSystem,
		string fullFilePath,
		string friendlyName,
		string givenPath,
		string currentDirectory,
		bool isRoot,
		string currentDirectoryWithSeparator)
	{
		if (isRoot)
		{
			fullFilePath = fullFilePath.Substring(currentDirectory.Length);
		}
		else if (fullFilePath.Equals(
			currentDirectoryWithSeparator,
			fileSystem.Execute.StringComparisonMode))
		{
			fullFilePath = ".";
		}
		else if (fullFilePath.StartsWith(
			currentDirectoryWithSeparator,
			fileSystem.Execute.StringComparisonMode))
		{
			fullFilePath = fullFilePath.Substring(currentDirectory.Length + 1);
		}
		else if (friendlyName.EndsWith(
			$"{fileSystem.Execute.Path.DirectorySeparatorChar}..",
			StringComparison.Ordinal))
		{
			fullFilePath = "..";
		}
		else
		{
			string? parentName = currentDirectory;
			while (parentName != null &&
			       !fullFilePath.StartsWith(
				       parentName + fileSystem.Execute.Path.DirectorySeparatorChar,
				       fileSystem.Execute.StringComparisonMode))
			{
				parentName = fileSystem.Execute.Path.GetDirectoryName(parentName);
				int lastIndex =
					givenPath.LastIndexOf(fileSystem.Execute.Path.DirectorySeparatorChar, StringComparison.Ordinal);
				if (lastIndex >= 0)
				{
					givenPath = givenPath.Substring(0, lastIndex);
				}
			}

			if (parentName != null)
			{
				fullFilePath = fullFilePath.Substring(parentName.Length + 1);
			}
		}

		if (!fullFilePath.StartsWith(givenPath + fileSystem.Execute.Path.DirectorySeparatorChar,
			fileSystem.Execute.StringComparisonMode))
		{
			return fileSystem.Execute.Path.Combine(givenPath, fullFilePath);
		}

		return fullFilePath;
	}

	/// <summary>
	///     Ignores all registrations on the <see cref="MockFileSystem.Statistics" /> until the return value is disposed.
	/// </summary>
	internal static IDisposable IgnoreStatistics(this IFileSystem fileSystem)
		=> FileSystemRegistration.Ignore();

	/// <summary>
	///     Returns the shared <see cref="IRandom" /> instance from the <paramref name="fileSystem" />, if it is a
	///     <see cref="MockFileSystem" />, otherwise <see cref="RandomFactory.Shared" />.
	/// </summary>
	internal static IRandom RandomOrDefault(this IFileSystem fileSystem)
	{
		if (fileSystem is MockFileSystem mockFileSystem)
		{
			return mockFileSystem.RandomSystem.Random.Shared;
		}

		return RandomFactory.Shared;
	}
}

using System;
using System.IO;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.Statistics;
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
		IStorageLocation destinationLocation =
			fileSystem.Storage.GetLocation(destinationPath, location.FriendlyName);
		return destinationLocation;
	}

	/// <summary>
	///     Returns the relative subdirectory path from <paramref name="fullFilePath" /> to the <paramref name="givenPath" />.
	/// </summary>
	internal static string GetSubdirectoryPath(this MockFileSystem fileSystem,
		string fullFilePath,
		string givenPath)
	{
		if (fileSystem.Execute.Path.IsPathRooted(givenPath))
		{
			return fullFilePath;
		}

		string currentDirectory = fileSystem.Execute.Path.GetFullPath(givenPath);
		if (string.Equals(currentDirectory, string.Empty.PrefixRoot(fileSystem),
			fileSystem.Execute.StringComparisonMode))
		{
			fullFilePath = fullFilePath.Substring(currentDirectory.Length);
		}
		else if (fullFilePath.StartsWith(currentDirectory + fileSystem.Execute.Path.DirectorySeparatorChar,
			fileSystem.Execute.StringComparisonMode))
		{
			fullFilePath = fullFilePath.Substring(currentDirectory.Length + 1);
		}
		else
		{
			string? parentName = currentDirectory;
			while (parentName != null &&
			       !fullFilePath.StartsWith(parentName + fileSystem.Execute.Path.DirectorySeparatorChar,
				       fileSystem.Execute.StringComparisonMode))
			{
				parentName = fileSystem.Execute.Path.GetDirectoryName(parentName);
				int lastIndex = givenPath.LastIndexOf(fileSystem.Execute.Path.DirectorySeparatorChar);
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
	///     Ignores all registrations on the <paramref name="statisticsGate" /> until the return value is disposed.
	/// </summary>
	internal static IDisposable Ignore(this IStatisticsGate statisticsGate)
	{
		statisticsGate.TryGetLock(out IDisposable? release);
		return release;
	}

	/// <summary>
	///     Ignores all registrations on the <see cref="MockFileSystem.Statistics" /> until the return value is disposed.
	/// </summary>
	internal static IDisposable IgnoreStatistics(this IFileSystem fileSystem)
	{
		if (fileSystem is MockFileSystem mockFileSystem)
		{
			return mockFileSystem.StatisticsRegistration.Ignore();
		}

		return new NoOpDisposable();
	}

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Storage;

internal static class StorageExtensions
{
	public static AdjustedLocation AdjustLocationFromSearchPattern(
		this IStorage storage, MockFileSystem fileSystem, string path, string searchPattern)
	{
		if (searchPattern == null)
		{
			throw new ArgumentNullException(nameof(searchPattern));
		}

		if (fileSystem.Execute.IsNetFramework &&
		    searchPattern.EndsWith("..", StringComparison.Ordinal))
		{
			throw ExceptionFactory.SearchPatternCannotContainTwoDots();
		}

		IStorageLocation location = storage.GetLocation(path.EnsureValidFormat(fileSystem));
		string givenPath = path.StartsWith(@"\", StringComparison.Ordinal) ? path : location.FriendlyName;
		if (searchPattern.StartsWith("..", StringComparison.Ordinal))
		{
			Stack<string> parentDirectories = new();
			StringBuilder givenPathPrefix = new();

			while (searchPattern.StartsWith(
				       ".." + fileSystem.Execute.Path.DirectorySeparatorChar,
				       StringComparison.Ordinal) ||
			       searchPattern.StartsWith(
				       ".." + fileSystem.Execute.Path.AltDirectorySeparatorChar,
				       StringComparison.Ordinal))
			{
				if (fileSystem.Execute.IsNetFramework)
				{
					throw ExceptionFactory.SearchPatternCannotContainTwoDots();
				}

				parentDirectories.Push(fileSystem.Execute.Path.GetFileName(location.FullPath));
				location = location.GetParent() ??
				           throw new UnauthorizedAccessException(
					           $"The searchPattern '{searchPattern}' has too many '../' for path '{path}'");
				#pragma warning disable CA1846
				givenPathPrefix.Append(searchPattern, 0, 3);
				#pragma warning restore CA1846
				searchPattern = searchPattern.Substring(3);
			}

			if (parentDirectories.Any())
			{
				givenPathPrefix.Length--;
				givenPath = fileSystem.Execute.Path.Combine(
					givenPath,
					givenPathPrefix.ToString(),
					fileSystem.Execute.Path.Combine(parentDirectories.ToArray()));
			}
		}

		return new AdjustedLocation(location, searchPattern, givenPath);
	}

	internal sealed class AdjustedLocation
	{
		public string GivenPath { get; }

		public IStorageLocation Location { get; }
		public string SearchPattern { get; }

		public AdjustedLocation(IStorageLocation location, string searchPattern,
			string givenPath)
		{
			Location = location;
			SearchPattern = searchPattern;
			GivenPath = givenPath;
		}
	}
}

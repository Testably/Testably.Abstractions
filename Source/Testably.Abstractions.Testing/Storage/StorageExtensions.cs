using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Storage;

internal static class StorageExtensions
{
	public static AdjustedLocation AdjustLocationFromSearchPattern(
		this IStorage storage, string path, string searchPattern)
	{
		if (searchPattern == null)
		{
			throw new ArgumentNullException(nameof(searchPattern));
		}

		Execute.OnNetFrameworkIf(searchPattern.EndsWith(".."),
			() => throw ExceptionFactory.SearchPatternCannotContainTwoDots());

		IStorageLocation location = storage.GetLocation(path);
		string givenPath = location.FriendlyName;
		if (searchPattern.StartsWith(".."))
		{
			Stack<string> parentDirectories = new();
			StringBuilder givenPathPrefix = new();

			while (searchPattern.StartsWith(".." + Path.DirectorySeparatorChar) ||
			       searchPattern.StartsWith(".." + Path.AltDirectorySeparatorChar))
			{
				Execute.OnNetFramework(
					() => throw ExceptionFactory.SearchPatternCannotContainTwoDots());
				parentDirectories.Push(Path.GetFileName(location.FullPath));
				location = location.GetParent() ??
				           throw new UnauthorizedAccessException(
					           $"The searchPattern '{searchPattern}' has too many '../' for path '{path}'");
				#pragma warning disable CA1846
				givenPathPrefix.Append(searchPattern.Substring(0, 3));
				#pragma warning restore CA1846
				searchPattern = searchPattern.Substring(3);
			}

			if (parentDirectories.Any())
			{
				givenPathPrefix.Length--;
				givenPath = Path.Combine(
					givenPath,
					givenPathPrefix.ToString(),
					Path.Combine(parentDirectories.ToArray()));
			}
		}

		return new AdjustedLocation(location, searchPattern, givenPath);
	}

	internal class AdjustedLocation
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			List<string> parentDirectories = new();

			while (searchPattern.StartsWith(".." + Path.DirectorySeparatorChar) ||
			       searchPattern.StartsWith(".." + Path.AltDirectorySeparatorChar))
			{
				Execute.OnNetFramework(
					() => throw ExceptionFactory.SearchPatternCannotContainTwoDots());
				parentDirectories.Add(Path.GetFileName(location.FullPath));
				location = location.GetParent() ?? throw new Exception("foo");
				searchPattern = searchPattern.Substring(3);
			}

			if (parentDirectories.Any())
			{
				givenPath = Path.Combine(
					givenPath,
					Path.Combine(parentDirectories.Select(_ => "..").ToArray()),
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
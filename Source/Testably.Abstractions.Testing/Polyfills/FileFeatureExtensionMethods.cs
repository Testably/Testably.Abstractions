#if !FEATURE_PATH_ADVANCED
using System.Diagnostics.CodeAnalysis;
using Testably.Abstractions.Testing.Helpers;

// ReSharper disable once CheckNamespace
namespace Testably.Abstractions.Testing;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class FileFeatureExtensionMethods
{
	internal static string TrimEndingDirectorySeparator(
		MockFileSystem fileSystem,
		string path, char directorySeparatorChar, char altDirectorySeparatorChar)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}

		string trimmed = path.TrimEnd(directorySeparatorChar,
			altDirectorySeparatorChar);

		return fileSystem.Execute.OnWindows(
			       () =>
			       {
				       if (trimmed.Length == 2
				           && char.IsLetter(trimmed[0])
				           && trimmed[1] == ':')
				       {
					       return trimmed + directorySeparatorChar;
				       }

				       return null;
			       },
			       () =>
			       {
				       if ((path[0] == directorySeparatorChar ||
				            path[0] == altDirectorySeparatorChar)
				           && trimmed == "")
				       {
					       return directorySeparatorChar.ToString();
				       }

				       return null;
			       })
		       ?? trimmed;
	}
}
#endif

#if !FEATURE_PATH_ADVANCED
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Testably.Abstractions.Testing;

/// <summary>
///	 Provides extension methods to simplify writing platform independent tests.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class FileFeatureExtensionMethods
{
	/// <summary>
	///	 Trims one trailing directory separator beyond the root of the path.
	/// </summary>
	internal static string TrimEndingDirectorySeparator(
		this IFileSystem.IPath pathSystem,
		string path)
	{
		return TrimEndingDirectorySeparator(path, pathSystem.DirectorySeparatorChar,
			pathSystem.AltDirectorySeparatorChar);
	}

	internal static string TrimEndingDirectorySeparator(
		string path, char directorySeparatorChar, char altDirectorySeparatorChar)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}

		string trimmed = path.TrimEnd(directorySeparatorChar,
			altDirectorySeparatorChar);

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			if (trimmed.Length == 2
				&& char.IsLetter(trimmed[0])
				&& trimmed[1] == ':')
			{
				return trimmed + directorySeparatorChar;
			}
		}
		else
		{
			if ((path[0] == directorySeparatorChar ||
				 path[0] == altDirectorySeparatorChar)
				&& trimmed == "")
			{
				return directorySeparatorChar.ToString();
			}
		}

		return trimmed;
	}
}
#endif
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static class FilePlatformIndependenceExtensions
{
	private static readonly Regex PathTransformRegex = new(@"^[a-zA-Z]:(?<path>.*)$");

	/// <summary>
	///     Normalizes the given path so that it works on all platforms.
	/// </summary>
	[return: NotNullIfNotNull("path")]
	public static string? NormalizePath(this string? path)
		=> path != null && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? PathTransformRegex
			   .Replace(path, "${path}")
			   .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
			: path?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

	/// <summary>
	///     Normalizes the given path so that it works on all platforms.
	/// </summary>
	[return: NotNullIfNotNull("path")]
	public static string? PrefixRoot(this string? path, char driveLetter = 'C')
	{
		if (path == null)
		{
			return path;
		}

		if (Path.IsPathRooted(path))
		{
			return path;
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return driveLetter + ":\\" + path;
		}

		return "/" + path;
	}
}
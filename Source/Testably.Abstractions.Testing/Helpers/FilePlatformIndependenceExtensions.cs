using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace Testably.Abstractions.Testing.Helpers;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
internal static class FilePlatformIndependenceExtensions
{
	#pragma warning disable SYSLIB1045
	private static readonly Regex PathTransformRegex = new(@"^[a-zA-Z]:(?<path>.*)$");
	#pragma warning restore SYSLIB1045

	private static readonly char DefaultDrive = GetDefaultDrive();

	private static char GetDefaultDrive()
		=> (Path.GetPathRoot(Directory.GetCurrentDirectory()) ?? "C:")[0];

	/// <summary>
	///     Normalizes the given path so that it works on all platforms.
	/// </summary>
	[return: NotNullIfNotNull("path")]
	public static string? NormalizePath(this string? path)
	{
		if (path == null)
		{
			return null;
		}

		return Execute.OnWindows(
			() => path
				.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
			() => PathTransformRegex
				.Replace(path, "${path}")
				.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
	}

	/// <summary>
	///     Normalizes the given path so that it works on all platforms.
	/// </summary>
	[return: NotNullIfNotNull("path")]
	public static string? PrefixRoot(this string? path, char? driveLetter = null)
	{
		if (path == null)
		{
			return path;
		}

		if (Path.IsPathRooted(path))
		{
			return path;
		}

		driveLetter ??= DefaultDrive;

		return Execute.OnWindows(
			() => driveLetter + ":\\" + path,
			() => "/" + path);
	}
}

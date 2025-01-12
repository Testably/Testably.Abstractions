using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Testably.Abstractions.Testing.Helpers;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
internal static class FilePlatformIndependenceExtensions
{
	#pragma warning disable SYSLIB1045
	#pragma warning disable MA0110
	private static readonly Regex PathTransformRegex = new("^[a-zA-Z]:(?<path>.*)$",
		RegexOptions.None,
		TimeSpan.FromMilliseconds(1000));
	#pragma warning restore MA0110
	#pragma warning restore SYSLIB1045

	/// <summary>
	///     Normalizes the given path so that it works on all platforms.
	/// </summary>
	[return: NotNullIfNotNull("path")]
	public static string? NormalizePath(this string? path, MockFileSystem fileSystem)
	{
		if (path == null)
		{
			return null;
		}

		if (fileSystem.Execute.IsWindows)
		{
			return path
				.Replace(fileSystem.Execute.Path.AltDirectorySeparatorChar,
					fileSystem.Execute.Path.DirectorySeparatorChar);
		}

		return PathTransformRegex
			.Replace(path, "${path}")
			.Replace(fileSystem.Execute.Path.AltDirectorySeparatorChar,
				fileSystem.Execute.Path.DirectorySeparatorChar);
	}

	/// <summary>
	///     Normalizes the given path so that it works on all platforms.
	/// </summary>
	[return: NotNullIfNotNull("path")]
	public static string? PrefixRoot(this string? path, MockFileSystem fileSystem,
		char driveLetter = 'C')
	{
		if (path == null)
		{
			return path;
		}

		if (fileSystem.Execute.Path.IsPathRooted(path))
		{
			return path;
		}

		if (fileSystem.Execute.IsWindows)
		{
			return driveLetter + ":\\" + path;
		}

		return "/" + path;
	}
}

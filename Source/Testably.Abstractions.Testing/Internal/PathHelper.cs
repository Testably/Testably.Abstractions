using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Internal;

internal static class PathHelper
{
	private static readonly char[] AdditionalInvalidPathChars = { '*', '?' };

	/// <summary>
	///     Determines whether the given path contains illegal characters.
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal static bool HasIllegalCharacters(this string path, IFileSystem fileSystem)
	{
		char[] invalidPathChars = fileSystem.Path.GetInvalidPathChars();

		if (path.IndexOfAny(invalidPathChars) >= 0)
		{
			return true;
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return path.IndexOfAny(AdditionalInvalidPathChars) >= 0;
		}

		return false;
	}

	internal static string
		NormalizeAndTrimPath(this string path, IFileSystem fileSystem)
		=> fileSystem.Path
		   .TrimEndingDirectorySeparator(path.NormalizePath())
		   .TrimOnWindows();

	internal static string RemoveLeadingDot(this string path)
	{
		while (path.StartsWith($".{Path.DirectorySeparatorChar}"))
		{
			path = path.Substring(2);
		}

		return path;
	}

	internal static void ThrowCommonExceptionsIfPathIsInvalid(
		[NotNull] this string? path, IFileSystem fileSystem)
	{
		if (path == null)
		{
			throw new ArgumentNullException(nameof(path));
		}

		if (path.Length == 0)
		{
			throw ExceptionFactory.PathCannotBeEmpty();
		}
#pragma warning disable CA2249 // Consider using String.Contains with char instead of String.IndexOf not possible in .NETSTANDARD2.0
		if (path.IndexOf('\0') >= 0)
#pragma warning restore CA2249
		{
			throw ExceptionFactory.PathHasIllegalCharacters(path);
		}

		if (path.HasIllegalCharacters(fileSystem))
		{
			if (Framework.IsNetFramework)
			{
				throw ExceptionFactory.PathHasIllegalCharacters(path);
			}

			throw ExceptionFactory.PathHasIncorrectSyntax(
				fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(),
					path));
		}
	}

	[ExcludeFromCodeCoverage]
	internal static string TrimOnWindows(this string path)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return path.TrimEnd(' ');
		}

		return path;
	}
}
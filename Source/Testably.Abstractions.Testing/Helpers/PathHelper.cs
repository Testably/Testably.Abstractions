using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Testing.Helpers;

internal static class PathHelper
{
	private static readonly char[] AdditionalInvalidPathChars =
	{
		'*', '?'
	};

	internal static readonly string UncPrefix = new(Path.DirectorySeparatorChar, 2);

	internal static readonly string UncAltPrefix = new(Path.AltDirectorySeparatorChar, 2);

	/// <summary>
	///     Determines whether the given path contains illegal characters.
	/// </summary>
	internal static bool HasIllegalCharacters(this string path, IFileSystem fileSystem)
	{
		char[] invalidPathChars = fileSystem.Path.GetInvalidPathChars();

		if (path.IndexOfAny(invalidPathChars) >= 0)
		{
			return true;
		}

		return Execute.OnWindows(
			() => path.IndexOfAny(AdditionalInvalidPathChars) >= 0,
			() => false);
	}

	/// <summary>
	///     Returns true if the path is effectively empty for the current OS.
	///     For unix, this is empty or null. For Windows, this is empty, null, or
	///     just spaces ((char)32).
	/// </summary>
	internal static bool IsEffectivelyEmpty(this string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return true;
		}

		return Execute.OnWindows(
			() => Execute.OnNetFramework(
				() => string.IsNullOrWhiteSpace(path),
				() =>
				{
					foreach (char c in path)
					{
						if (c != ' ')
						{
							return false;
						}
					}

					return true;
				}),
			() => false);
	}

	internal static bool IsUncPath([NotNullWhen(true)] this string? path)
	{
		if (path == null)
		{
			return false;
		}

		return Execute.OnWindows(
			() => path.StartsWith(UncPrefix) || path.StartsWith(UncAltPrefix),
			() => path.StartsWith(UncPrefix));
	}

	internal static string EnsureValidFormat(
		[NotNull] this string? path,
		IFileSystem fileSystem,
		string? paramName = null,
		bool? includeIsEmptyCheck = null)
	{
		CheckPathArgument(path, paramName ?? nameof(path),
			includeIsEmptyCheck ?? Execute.IsWindows);
		CheckPathCharacters(path, fileSystem, paramName ?? nameof(path), null);
		return path;
	}

	internal static string EnsureValidArgument(
		[NotNull] this string? path, IFileSystem fileSystem, string? paramName = null)
	{
		CheckPathArgument(path, paramName ?? nameof(path), Execute.IsWindows);
		return path;
	}

	internal static void ThrowCommonExceptionsIfPathToTargetIsInvalid(
		[NotNull] this string? pathToTarget, IFileSystem fileSystem)
	{
		CheckPathArgument(pathToTarget, nameof(pathToTarget), false);
		CheckPathCharacters(pathToTarget, fileSystem, nameof(pathToTarget), -2147024713);
	}

	/// <summary>
	///     Get the <see cref="IPath.GetFullPath(string)" /> of the <paramref name="path" />
	///     if the <paramref name="path" /> is not <see langword="null" /> or white space.<br />
	///     Otherwise an empty string if the <paramref name="path" /> is <see langword="null" />
	///     or the <paramref name="path" /> itself.
	/// </summary>
	internal static string GetFullPathOrWhiteSpace(this string? path, IFileSystem fileSystem)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return path ?? string.Empty;
		}

		return fileSystem.Path.GetFullPath(path);
	}

	private static void CheckPathArgument([NotNull] string? path, string paramName,
		bool includeIsEmptyCheck)
	{
		if (path == null)
		{
			throw new ArgumentNullException(paramName);
		}

		if (path.Length == 0)
		{
			throw ExceptionFactory.PathCannotBeEmpty(paramName);
		}

		if (includeIsEmptyCheck && path.Trim() == string.Empty)
		{
			throw ExceptionFactory.PathIsEmpty(paramName);
		}
	}

	private static void CheckPathCharacters(string path, IFileSystem fileSystem,
		string paramName, int? hResult)
	{
		#pragma warning disable CA2249 // Consider using String.Contains with char instead of String.IndexOf not possible in .NETSTANDARD2.0
		if (path.IndexOf('\0') >= 0)
			#pragma warning restore CA2249
		{
			throw ExceptionFactory.PathHasIllegalCharacters(path, paramName, hResult);
		}

		if (path.HasIllegalCharacters(fileSystem))
		{
			Execute.OnNetFramework(()
				=> throw ExceptionFactory.PathHasIllegalCharacters(path, paramName,
					hResult));

			throw ExceptionFactory.PathHasIncorrectSyntax(
				fileSystem.Path.GetFullPath(path), hResult);
		}

		Execute.OnWindowsIf(path.LastIndexOf(':') > 1 &&
		                    path.LastIndexOf(':') < path.IndexOf(Path.DirectorySeparatorChar),
			() => throw ExceptionFactory.PathHasIncorrectSyntax(
				fileSystem.Path.GetFullPath(path), hResult));
	}

	internal static string TrimOnWindows(this string path)
		=> Execute.OnWindows(
			() => path.TrimEnd(' '),
			() => path);
}

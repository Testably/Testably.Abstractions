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

	internal static string EnsureValidArgument(
		[NotNull] this string? path, MockFileSystem fileSystem, string? paramName = null)
	{
		CheckPathArgument(fileSystem.Execute, path, paramName ?? nameof(path),
			fileSystem.Execute.IsWindows);
		return path;
	}

	internal static string EnsureValidFormat(
		[NotNull] this string? path,
		MockFileSystem fileSystem,
		string? paramName = null,
		bool? includeIsEmptyCheck = null)
	{
		CheckPathArgument(fileSystem.Execute, path, paramName ?? nameof(path),
			includeIsEmptyCheck ?? fileSystem.Execute.IsWindows);
		CheckPathCharacters(path, fileSystem, paramName ?? nameof(path), null);
		return path;
	}

	/// <summary>
	///     Get the <see cref="IPath.GetFullPath(string)" /> of the <paramref name="path" />
	///     if the <paramref name="path" /> is not <see langword="null" /> or white space.<br />
	///     Otherwise, an empty string if the <paramref name="path" /> is <see langword="null" />
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

	/// <summary>
	///     Determines whether the given path contains illegal characters.
	/// </summary>
	internal static bool HasIllegalCharacters(this string path, MockFileSystem fileSystem)
	{
		char[] invalidPathChars = fileSystem.Path.GetInvalidPathChars();

		if (path.IndexOfAny(invalidPathChars) >= 0)
		{
			return true;
		}

		return fileSystem.Execute.OnWindows(
			() => path.IndexOfAny(AdditionalInvalidPathChars) >= 0,
			() => false);
	}

	/// <summary>
	///     Returns true if the path is effectively empty for the current OS.
	///     For unix, this is empty or null. For Windows, this is empty, null, or
	///     just spaces ((char)32).
	/// </summary>
	internal static bool IsEffectivelyEmpty(this string? path, MockFileSystem fileSystem)
	{
		if (string.IsNullOrEmpty(path))
		{
			return true;
		}

		return fileSystem.Execute.OnWindows(
			() => fileSystem.Execute.OnNetFramework(
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

	internal static bool IsUncPath([NotNullWhen(true)] this string? path, MockFileSystem fileSystem)
	{
		if (path == null)
		{
			return false;
		}

		return fileSystem.Execute.OnWindows(
			() => path.StartsWith(new string(fileSystem.Path.DirectorySeparatorChar, 2)) ||
			      path.StartsWith(new string(fileSystem.Path.AltDirectorySeparatorChar, 2)),
			() => path.StartsWith(new string(fileSystem.Path.DirectorySeparatorChar, 2)));
	}

	internal static void ThrowCommonExceptionsIfPathToTargetIsInvalid(
		[NotNull] this string? pathToTarget, MockFileSystem fileSystem)
	{
		CheckPathArgument(fileSystem.Execute, pathToTarget, nameof(pathToTarget), false);
		CheckPathCharacters(pathToTarget, fileSystem, nameof(pathToTarget), -2147024713);
	}

	internal static string TrimOnWindows(this string path, MockFileSystem fileSystem)
		=> fileSystem.Execute.OnWindows(
			() => path.TrimEnd(' '),
			() => path);

	private static void CheckPathArgument(Execute execute, [NotNull] string? path, string paramName,
		bool includeIsEmptyCheck)
	{
		if (path == null)
		{
			throw new ArgumentNullException(paramName);
		}

		if (path.Length == 0)
		{
			throw ExceptionFactory.PathCannotBeEmpty(execute, paramName);
		}

		if (includeIsEmptyCheck && path.Trim() == string.Empty)
		{
			throw ExceptionFactory.PathIsEmpty(paramName);
		}
	}

	private static void CheckPathCharacters(string path, MockFileSystem fileSystem,
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
			fileSystem.Execute.OnNetFramework(()
				=> throw ExceptionFactory.PathHasIllegalCharacters(path, paramName,
					hResult));

			throw ExceptionFactory.PathHasIncorrectSyntax(
				fileSystem.Path.GetFullPath(path), hResult);
		}

		fileSystem.Execute.OnWindowsIf(path.LastIndexOf(':') > 1 &&
		                               path.LastIndexOf(':') <
		                               path.IndexOf(Path.DirectorySeparatorChar),
			() => throw ExceptionFactory.PathHasIncorrectSyntax(
				fileSystem.Path.GetFullPath(path), hResult));
	}
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing.Internal;

internal static class PathHelper
{
    private static readonly char[] AdditionalInvalidPathChars = { '*', '?' };

    /// <summary>
    ///     Determines whether the given path contains illegal characters.
    /// </summary>
    internal static bool HasIllegalCharacters(this string path, IFileSystem fileSystem)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        char[] invalidPathChars = fileSystem.Path.GetInvalidPathChars();

        if (path.IndexOfAny(invalidPathChars) >= 0)
        {
            return true;
        }

        return path.IndexOfAny(AdditionalInvalidPathChars) >= 0;
    }

    internal static string
        NormalizeAndTrimPath(this string path, IFileSystem fileSystem)
        => fileSystem.Path
           .TrimEndingDirectorySeparator(path.NormalizePath())
           .TrimOnWindows();

    internal static string TrimOnWindows(this string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return path.TrimEnd(' ');
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
            throw new ArgumentException(
                "Path cannot be the empty string or all whitespace.", nameof(path));
        }

        if (path.IndexOf('\0') >= 0)
        {
            throw new ArgumentException("Illegal characters in path.", nameof(path));
        }
    }
}
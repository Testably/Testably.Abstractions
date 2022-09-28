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
    }

    internal static string TrimOnWindows(this string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return path.TrimEnd(' ');
        }

        return path;
    }
}
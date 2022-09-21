using System;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing.Internal;

internal static class EnumerationOptionsHelper
{
    /// <summary>
    ///     For internal use. These are the options we want to use if calling the existing Directory/File APIs where you don't
    ///     explicitly specify EnumerationOptions.
    /// </summary>
    internal static EnumerationOptions Compatible { get; } =
        new()
        {
            MatchType = MatchType.Win32,
            AttributesToSkip = 0,
            IgnoreInaccessible = false
        };

    private static EnumerationOptions CompatibleRecursive { get; } =
        new()
        {
            RecurseSubdirectories = true,
            MatchType = MatchType.Win32,
            AttributesToSkip = 0,
            IgnoreInaccessible = false
        };

    public static bool MatchesPattern(EnumerationOptions enumerationOptions, string name,
                                      string searchString)
    {
        bool ignoreCase =
            (enumerationOptions.MatchCasing == MatchCasing.PlatformDefault &&
             !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            || enumerationOptions.MatchCasing == MatchCasing.CaseInsensitive;

        return enumerationOptions.MatchType switch
        {
            MatchType.Simple => MatchPattern(searchString, name, ignoreCase, false),
            MatchType.Win32 => MatchPattern(searchString, name, ignoreCase, true),
            _ => throw new ArgumentOutOfRangeException(nameof(enumerationOptions)),
        };
    }

    /// <summary>
    ///     Converts SearchOptions to FindOptions. Throws if undefined SearchOption.
    /// </summary>
    internal static EnumerationOptions FromSearchOption(SearchOption searchOption)
    {
        if (searchOption != SearchOption.TopDirectoryOnly &&
            searchOption != SearchOption.AllDirectories)
        {
            throw new ArgumentOutOfRangeException(nameof(searchOption));
        }

        return searchOption == SearchOption.AllDirectories
            ? CompatibleRecursive
            : Compatible;
    }

    private static bool MatchPattern(string expression,
                                     string name,
                                     bool ignoreCase,
                                     bool useExtendedWildcards)
    {
        if (useExtendedWildcards)
        {
            expression = SimplifyExpression(expression);
        }

        if (useExtendedWildcards)
        {
            return FileSystemName.MatchesWin32Expression(expression,
                name, ignoreCase);
        }

        return FileSystemName.MatchesSimpleExpression(expression,
            name, ignoreCase);
    }

    /// <summary>
    ///     This method was inspired by the `FileSystemEnumerableFactory.NormalizeInputs` in .NET6.
    ///     <para />
    ///     <seealso
    ///         href="https://github.com/dotnet/runtime/blob/174e04336e4a073416cb0baebb2be72728b9d64b/src/libraries/System.Private.CoreLib/src/System/IO/Enumeration/FileSystemEnumerableFactory.cs#L27" />
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private static string SimplifyExpression(string expression)
    {
        char[] unixEscapeChars = { '\\', '"', '<', '>' };
        if (expression == "*")
        {
            return expression;
        }

        if (string.IsNullOrEmpty(expression) || expression == "." || expression == "*.*")
        {
            return "*";
        }

        if (Path.DirectorySeparatorChar != '\\' &&
            expression.IndexOfAny(unixEscapeChars) != -1)
        {
            // Backslash isn't the default separator, need to escape (e.g. Unix)
            expression = expression.Replace("\\", "\\\\");

            // Also need to escape the other special wild characters ('"', '<', and '>')
            expression = expression.Replace("\"", "\\\"");
            expression = expression.Replace(">", "\\>");
            expression = expression.Replace("<", "\\<");
        }

        // Need to convert the expression to match Win32 behavior
        return FileSystemName.TranslateWin32Expression(expression);
    }
}
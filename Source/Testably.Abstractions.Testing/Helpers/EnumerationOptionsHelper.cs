using System;
using System.IO;
using System.IO.Enumeration;

namespace Testably.Abstractions.Testing.Helpers;

internal static class EnumerationOptionsHelper
{
	/// <summary>
	///     The default search pattern, when none is provided.<br />
	///     It matches any file or directory.
	/// </summary>
	internal const string DefaultSearchPattern = "*";

	/// <summary>
	///     For internal use. These are the options we want to use if calling the existing Directory/File APIs where you don't
	///     explicitly specify EnumerationOptions.
	///     <para />
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/IO/EnumerationOptions.cs#L24" />
	/// </summary>
	internal static EnumerationOptions Compatible { get; } =
		new()
		{
			MatchType = MatchType.Win32,
			AttributesToSkip = 0,
			IgnoreInaccessible = false
		};

	/// <summary>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/IO/EnumerationOptions.cs#L27" />
	/// </summary>
	private static EnumerationOptions CompatibleRecursive { get; } =
		new()
		{
			RecurseSubdirectories = true,
			MatchType = MatchType.Win32,
			AttributesToSkip = 0,
			IgnoreInaccessible = false
		};

	/// <summary>
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/IO/Enumeration/FileSystemEnumerableFactory.cs#L107" />
	/// </summary>
	public static bool MatchesPattern(Execute execute, EnumerationOptions enumerationOptions,
		string name,
		string searchString)
	{
		bool ignoreCase =
			(enumerationOptions.MatchCasing == MatchCasing.PlatformDefault &&
			 execute.IsWindows)
			|| enumerationOptions.MatchCasing == MatchCasing.CaseInsensitive;

		return enumerationOptions.MatchType switch
		{
			MatchType.Simple => MatchPattern(execute, searchString, name, ignoreCase, false),
			MatchType.Win32 => MatchPattern(execute, searchString, name, ignoreCase, true),
			_ => throw new ArgumentOutOfRangeException(nameof(enumerationOptions)),
		};
	}

	/// <summary>
	///     Converts SearchOptions to FindOptions. Throws if undefined SearchOption.
	///     <para />
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/IO/EnumerationOptions.cs#L46" />
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

	private static bool MatchPattern(Execute execute,
		string searchString,
		string name,
		bool ignoreCase,
		bool useExtendedWildcards)
	{
		if (execute.IsNetFramework && searchString == "")
		{
			return false;
		}

		if (useExtendedWildcards)
		{
			searchString = SimplifyExpression(execute, searchString);
			return FileSystemName.MatchesWin32Expression(searchString,
				name, ignoreCase);
		}

		return FileSystemName.MatchesSimpleExpression(searchString,
			name, ignoreCase);
	}

	/// <summary>
	///     This method was inspired by the `FileSystemEnumerableFactory.NormalizeInputs` in .NET6.
	///     <para />
	///     <seealso
	///         href="https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/IO/Enumeration/FileSystemEnumerableFactory.cs#L37" />
	/// </summary>
	private static string SimplifyExpression(Execute execute, string searchString)
	{
		char[] unixEscapeChars =
		{
			'\\',
			'"',
			'<',
			'>'
		};
		if (searchString == DefaultSearchPattern)
		{
			return searchString;
		}

		if (searchString == "." || searchString == "*.*")
		{
			return DefaultSearchPattern;
		}

		if (!execute.IsNetFramework && string.IsNullOrEmpty(searchString))
		{
			return "*";
		}

		execute.NotOnWindowsIf(searchString.IndexOfAny(unixEscapeChars) != -1,
			() =>
			{
				// Backslash isn't the default separator, need to escape (e.g. Unix)
				searchString = searchString.Replace("\\", "\\\\");

				// Also need to escape the other special wild characters ('"', '<', and '>')
				searchString = searchString.Replace("\"", "\\\"");
				searchString = searchString.Replace(">", "\\>");
				searchString = searchString.Replace("<", "\\<");
			});

		// Need to convert the expression to match Win32 behavior
		return FileSystemName.TranslateWin32Expression(searchString);
	}
}

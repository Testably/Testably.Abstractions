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

	/// <summary>
	///     Validates the directory and expression strings.<br />
	///     If the expression string begins with a directory name, the directory name is moved and appended at the end of the
	///     directory string.
	///     <para />
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v8.0.0/src/libraries/System.Private.CoreLib/src/System/IO/Enumeration/FileSystemEnumerableFactory.cs#L27" />
	/// </summary>
	internal static void NormalizeInputs(Execute execute,
		ref string directory,
		ref string expression)
	{
		if (!expression.Contains(
			execute.Path.DirectorySeparatorChar,
			StringComparison.Ordinal))
		{
			return;
		}

		// We always allowed breaking the passed ref directory and filter to be separated
		// any way the user wanted. Looking for "C:\foo\*.cs" could be passed as "C:\" and
		// "foo\*.cs" or "C:\foo" and "*.cs", for example. As such we need to combine and
		// split the inputs if the expression contains a directory separator.
		//
		// We also allowed for expression to be "foo\" which would translate to "foo\*".
		string? directoryName = execute.Path.GetDirectoryName(expression);
		if (directoryName?.Length > 0)
		{
			// Need to fix up the input paths
			directory = execute.Path.GetFullPath(
				execute.Path.Combine(directory, directoryName));
			expression = expression.Substring(directoryName.Length + 1);
		}
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
		[
			'\\',
			'"',
			'<',
			'>'
		];
		if (string.Equals(searchString, DefaultSearchPattern, StringComparison.Ordinal))
		{
			return searchString;
		}

		if (string.Equals(searchString, ".", StringComparison.Ordinal) ||
		    string.Equals(searchString, "*.*", StringComparison.Ordinal))
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
				searchString = searchString.Replace("\\", "\\\\", StringComparison.Ordinal);

				// Also need to escape the other special wild characters ('"', '<', and '>')
				searchString = searchString.Replace("\"", "\\\"", StringComparison.Ordinal);
				searchString = searchString.Replace(">", "\\>", StringComparison.Ordinal);
				searchString = searchString.Replace("<", "\\<", StringComparison.Ordinal);
			});

		// Need to convert the expression to match Win32 behavior
		return FileSystemName.TranslateWin32Expression(searchString);
	}
}

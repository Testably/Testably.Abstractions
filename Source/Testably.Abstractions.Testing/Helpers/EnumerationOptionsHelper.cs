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
	///     Validates the directory and expression strings to check that they have no invalid characters, any special DOS
	///     wildcard characters in Win32 in the expression get replaced with their proper escaped representation, and if the
	///     expression string begins with a directory name, the directory name is moved and appended at the end of the
	///     directory string.
	/// </summary>
	/// <param name="directory">A reference to a directory string that we will be checking for normalization.</param>
	/// <param name="expression">A reference to a expression string that we will be checking for normalization.</param>
	/// <param name="matchType">
	///     The kind of matching we want to check in the expression. If the value is Win32, we will replace
	///     special DOS wild characters to their safely escaped representation. This replacement does not affect the
	///     normalization status of the expression.
	/// </param>
	/// <returns>
	///     <cref langword="false" /> if the directory reference string get modified inside this function due to the
	///     expression beginning with a directory name. <cref langword="true" /> if the directory reference string was not
	///     modified.
	/// </returns>
	/// <exception cref="ArgumentException">
	///     The expression is a rooted path.
	///     -or-
	///     The directory or the expression reference strings contain a null character.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	///     The match type is out of the range of the valid MatchType enum values.
	/// </exception>
	internal static bool NormalizeInputs(
		ref string directory,
		ref string expression,
		MatchType matchType)
	{
		char[] unixEscapeChars = ['\\', '"', '<', '>'];

		// We always allowed breaking the passed ref directory and filter to be separated
		// any way the user wanted. Looking for "C:\foo\*.cs" could be passed as "C:\" and
		// "foo\*.cs" or "C:\foo" and "*.cs", for example. As such we need to combine and
		// split the inputs if the expression contains a directory separator.
		//
		// We also allowed for expression to be "foo\" which would translate to "foo\*".

		string? directoryName = Path.GetDirectoryName(expression);

		bool isDirectoryModified = true;

		if (directoryName?.Length > 0)
		{
			// Need to fix up the input paths
			directory = Path.Combine(directory, directoryName);
			expression = expression.Substring(directoryName.Length + 1);

			isDirectoryModified = false;
		}

		switch (matchType)
		{
			case MatchType.Win32:
				if (string.Equals(expression, "*", StringComparison.Ordinal))
				{
					// Most common case
					break;
				}

				if (string.IsNullOrEmpty(expression) ||
				    string.Equals(expression, ".", StringComparison.Ordinal) ||
				    string.Equals(expression, "*.*", StringComparison.Ordinal))
				{
					// Historically we always treated "." as "*"
					expression = "*";
				}
				else
				{
					// These all have special meaning in DOS name matching. '\' is the escaping character (which conveniently
					// is the directory separator and cannot be part of any path segment in Windows). The other three are the
					// special case wildcards that we'll convert some * and ? into. They're also valid as filenames on Unix,
					// which is not true in Windows and as such we'll escape any that occur on the input string.
					if (Path.DirectorySeparatorChar != '\\' &&
					    expression.IndexOfAny(unixEscapeChars) != -1)
					{
						// Backslash isn't the default separator, need to escape (e.g. Unix)
						expression = expression.Replace("\\", "\\\\", StringComparison.Ordinal);

						// Also need to escape the other special wild characters ('"', '<', and '>')
						expression = expression.Replace("\"", "\\\"", StringComparison.Ordinal);
						expression = expression.Replace(">", "\\>", StringComparison.Ordinal);
						expression = expression.Replace("<", "\\<", StringComparison.Ordinal);
					}

					// Need to convert the expression to match Win32 behavior
					expression = FileSystemName.TranslateWin32Expression(expression);
				}

				break;
			case MatchType.Simple:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(matchType));
		}

		return isDirectoryModified;
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

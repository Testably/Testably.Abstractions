using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Enumeration;

namespace Testably.Abstractions.Testing.Internal;

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
	[ExcludeFromCodeCoverage]
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
	[ExcludeFromCodeCoverage]
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
	[ExcludeFromCodeCoverage]
	public static bool MatchesPattern(EnumerationOptions enumerationOptions, string name,
	                                  string searchString)
	{
		bool ignoreCase =
			(enumerationOptions.MatchCasing == MatchCasing.PlatformDefault &&
			 Execute.IsWindows)
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
	///     <para />
	///     <see
	///         href="https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/IO/EnumerationOptions.cs#L46" />
	/// </summary>
	[ExcludeFromCodeCoverage]
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

	[ExcludeFromCodeCoverage]
	private static bool MatchPattern(string expression,
	                                 string name,
	                                 bool ignoreCase,
	                                 bool useExtendedWildcards)
	{
		if (Execute.IsNetFramework && expression == "")
		{
			return false;
		}

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
	///         href="https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Private.CoreLib/src/System/IO/Enumeration/FileSystemEnumerableFactory.cs#L37" />
	/// </summary>
	[ExcludeFromCodeCoverage]
	private static string SimplifyExpression(string expression)
	{
		char[] unixEscapeChars = { '\\', '"', '<', '>' };
		if (expression == DefaultSearchPattern)
		{
			return expression;
		}

		if (expression == "." || expression == "*.*")
		{
			return DefaultSearchPattern;
		}

		if (!Execute.IsNetFramework && string.IsNullOrEmpty(expression))
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
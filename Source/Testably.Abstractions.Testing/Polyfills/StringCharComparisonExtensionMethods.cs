#if !FEATURE_STRING_CHAR_COMPARISON
using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Testably.Abstractions.Polyfills;

/// <summary>
///     Provides the character overloads with a <see cref="StringComparison" />, which were added in .NET 11.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class StringCharComparisonExtensionMethods
{
	/// <summary>
	///     Determines whether the end of this string instance matches the specified character.
	/// </summary>
	internal static bool EndsWith(
		this string @this,
		char value,
		StringComparison comparisonType)
	{
		return @this.EndsWith(value);
	}

	/// <summary>
	///     Reports the zero-based index of the last occurrence of the specified character in this string.
	/// </summary>
	internal static int LastIndexOf(
		this string @this,
		char value,
		StringComparison comparisonType)
	{
		return @this.LastIndexOf(value);
	}

	/// <summary>
	///     Determines whether this string instance starts with the specified character.
	/// </summary>
	internal static bool StartsWith(
		this string @this,
		char value,
		StringComparison comparisonType)
	{
		return @this.StartsWith(value);
	}
}
#endif

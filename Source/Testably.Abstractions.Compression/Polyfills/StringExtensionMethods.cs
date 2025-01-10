#if NETSTANDARD2_0
using System;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions.Polyfills;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class StringExtensionMethods
{
	/// <summary>
	///     Determines whether the end of this string instance matches the specified character.
	/// </summary>
	internal static bool EndsWith(
		this string @this,
		char value)
	{
		#pragma warning disable MA0074
		return @this.EndsWith($"{value}");
		#pragma warning restore MA0074
	}

	/// <summary>
	///     Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.
	/// </summary>
	internal static string Replace(
		this string @this,
		// ReSharper disable once UnusedParameter.Global
		string oldValue, string? newValue, StringComparison comparisonType)
	{
		#pragma warning disable MA0074
		return @this.Replace(oldValue, newValue);
		#pragma warning restore MA0074
	}
}
#endif

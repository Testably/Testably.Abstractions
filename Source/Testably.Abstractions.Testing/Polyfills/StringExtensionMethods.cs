#if NETSTANDARD2_0
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Testably.Abstractions.Polyfills;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class StringExtensionMethods
{

	/// <summary>
	///     Returns a value indicating whether a specified character occurs within this string, using the specified comparison rules.
	/// </summary>
	/// <returns>
	///   <see langword="true" /> if the <paramref name="value" /> parameter occurs withing this string; otherwise, <see langword="false" />.
	/// </returns>
	internal static bool Contains(
		this string @this,
		char value,
		StringComparison comparisonType)
	{
		return @this.Contains(value);
	}

	/// <summary>
	///     Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.
	/// </summary>
	/// <returns>
	///   A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged.
	/// </returns>
	internal static string Replace(
		this string @this,
		string oldValue,
		string newValue,
		StringComparison comparisonType)
	{
		return @this.Replace(oldValue, newValue);
	}

	/// <summary>
	///     Determines whether the end of this string instance matches the specified character.
	/// </summary>
	internal static bool EndsWith(
		this string @this,
		char value)
	{
		return @this.EndsWith($"{value}", StringComparison.Ordinal);
	}
	
	/// <summary>
	///     Determines whether this string instance starts with the specified character.
	/// </summary>
	internal static bool StartsWith(
		this string @this,
		char value)
	{
		return @this.StartsWith($"{value}", StringComparison.Ordinal);
	}
}
#endif

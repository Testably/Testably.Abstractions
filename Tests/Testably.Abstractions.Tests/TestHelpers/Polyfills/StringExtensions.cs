#if NET48
// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
	public static bool Contains(this string @this, string value,
		StringComparison comparisonType)
	{
		#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'... this is the implementation of Contains!
		return @this.IndexOf(value, comparisonType) >= 0;
#pragma warning restore CA2249
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
}
#endif

#if NET472
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
}
#endif

#if !NET11_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;

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
}
#endif

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
}
#endif

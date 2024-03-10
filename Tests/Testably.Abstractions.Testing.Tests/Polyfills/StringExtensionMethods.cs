#if NET48
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Testably.Abstractions.Polyfills;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class StringExtensionMethods
{
	/// <summary>
	///     Reports the zero-based index of the first occurrence of the specified string in the current
	///     <see langword="string" /> object. A parameter specifies the type of search to use for the specified string.
	/// </summary>
	/// <returns>
	///     The index position of the <paramref name="value" /> parameter if that string is found, or <c>-1</c> if it is not.
	///     If <paramref name="value" /> is <see cref="string.Empty" />, the return value is <c>0</c>.
	/// </returns>
	internal static int IndexOf(
		this string @this,
		char value,
		StringComparison comparison)
	{
		return @this.IndexOf($"{value}", comparison);
	}
}
#endif

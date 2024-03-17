using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions.Testing.Helpers;

/// <summary>
///     Provides extension methods on <see cref="char" />.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class CharExtensionMethods
{
	/// <summary>Indicates whether a character is categorized as an ASCII letter.</summary>
	/// <param name="c">The character to evaluate.</param>
	/// <returns>true if <paramref name="c" /> is an ASCII letter; otherwise, false.</returns>
	/// <remarks>
	///     This determines whether the character is in the range 'A' through 'Z', inclusive,
	///     or 'a' through 'z', inclusive.
	/// </remarks>
	public static bool IsAsciiLetter(this char c) => (uint)((c | 0x20) - 'a') <= 'z' - 'a';
}

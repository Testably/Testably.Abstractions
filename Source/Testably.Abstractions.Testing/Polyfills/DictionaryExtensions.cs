#if NETSTANDARD2_0
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Testably.Abstractions.Polyfills;

[ExcludeFromCodeCoverage]
internal static class DictionaryExtensions
{
	/// <summary>
	///     Attempts to add the specified <paramref name="key" /> and <paramref name="value" /> to the dictionary.
	/// </summary>
	/// <returns>
	///     <see langword="true" /> if the key/value pair was added to the dictionary successfully; otherwise, <see langword="false" />.
	/// </returns>
	public static bool TryAdd<TKey, TValue>(
		this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
		where TKey : notnull
	{
		if (!dictionary.ContainsKey(key))
		{
			dictionary[key] = value;
			return true;
		}

		return false;
	}
}
#endif

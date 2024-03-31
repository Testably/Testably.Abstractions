#if NETSTANDARD2_0
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Testably.Abstractions.Polyfills;

[ExcludeFromCodeCoverage]
internal static class ConcurrentDictionaryExtensions
{
	/// <summary>
	///     Tries to get the value associated with the specified <paramref name="key" /> in the <paramref name="dictionary" />.
	/// </summary>
	public static TValue GetValueOrDefault<TKey, TValue>(
		this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
		where TKey : notnull
	{
		if (dictionary.TryGetValue(key, out TValue? d))
		{
			return d;
		}

		return default;
	}
}
#endif

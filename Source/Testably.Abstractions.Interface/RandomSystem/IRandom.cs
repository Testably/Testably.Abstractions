using System;

namespace Testably.Abstractions.RandomSystem;

/// <summary>
///     Abstractions for <see cref="Random" />.
/// </summary>
public interface IRandom
{
#if FEATURE_RANDOM_STRINGS
	/// <inheritdoc cref="Random.GetHexString(int, bool)" />
	string GetHexString(int stringLength, bool lowercase = false);
#endif

#if FEATURE_RANDOM_STRINGS
	/// <inheritdoc cref="Random.GetHexString(Span{char}, bool)" />
	void GetHexString(Span<char> destination, bool lowercase = false);
#endif
#if FEATURE_RANDOM_ITEMS
	/// <inheritdoc cref="Random.GetItems{T}(ReadOnlySpan{T}, Span{T})" />
	void GetItems<T>(ReadOnlySpan<T> choices, Span<T> destination);
#endif

#if FEATURE_RANDOM_ITEMS
	/// <inheritdoc cref="Random.GetItems{T}(T[], int)" />
	T[] GetItems<T>(T[] choices, int length);
#endif

#if FEATURE_RANDOM_ITEMS
	/// <inheritdoc cref="Random.GetItems{T}(ReadOnlySpan{T}, int)" />
	T[] GetItems<T>(ReadOnlySpan<T> choices, int length);
#endif

#if FEATURE_RANDOM_STRINGS
	/// <inheritdoc cref="Random.GetString(ReadOnlySpan{char}, int)" />
	string GetString(ReadOnlySpan<char> choices, int length);
#endif

	/// <inheritdoc cref="Random.Next()" />
	int Next();

	/// <inheritdoc cref="Random.Next(int)" />
	int Next(int maxValue);

	/// <inheritdoc cref="Random.Next(int, int)" />
	int Next(int minValue, int maxValue);

	/// <inheritdoc cref="Random.NextBytes(byte[])" />
	void NextBytes(byte[] buffer);

#if FEATURE_SPAN
	/// <inheritdoc cref="Random.NextBytes(Span{byte})" />
	void NextBytes(Span<byte> buffer);
#endif

	/// <inheritdoc cref="Random.NextDouble()" />
	double NextDouble();

#if FEATURE_RANDOM_ADVANCED
	/// <inheritdoc cref="Random.NextInt64()" />
	long NextInt64();
#endif

#if FEATURE_RANDOM_ADVANCED
	/// <inheritdoc cref="Random.NextInt64(long)" />
	long NextInt64(long maxValue);
#endif

#if FEATURE_RANDOM_ADVANCED
	/// <inheritdoc cref="Random.NextInt64(long, long)" />
	long NextInt64(long minValue, long maxValue);
#endif

#if FEATURE_RANDOM_ADVANCED
	/// <inheritdoc cref="Random.NextSingle()" />
	float NextSingle();
#endif

#if FEATURE_RANDOM_ITEMS
	/// <inheritdoc cref="Random.Shuffle{T}(T[])" />
	void Shuffle<T>(T[] values);
#endif

#if FEATURE_RANDOM_ITEMS
	/// <inheritdoc cref="Random.Shuffle{T}(Span{T})" />
	void Shuffle<T>(Span<T> values);
#endif
}

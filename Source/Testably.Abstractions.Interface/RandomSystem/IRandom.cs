using System;

namespace Testably.Abstractions.RandomSystem;

/// <summary>
///     Abstractions for <see cref="Random" />.
/// </summary>
public interface IRandom
{
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

	/// <inheritdoc cref="Random.NextInt64(long)" />
	long NextInt64(long maxValue);

	/// <inheritdoc cref="Random.NextInt64(long, long)" />
	long NextInt64(long minValue, long maxValue);

	/// <inheritdoc cref="Random.NextSingle()" />
	float NextSingle();
#endif
}

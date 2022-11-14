using System;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Helpers;

/// <summary>
///     Wrapper around <see cref="Random" />.
/// </summary>
public sealed class RandomWrapper : IRandom
{
	private readonly Random _instance;

	/// <summary>
	///     Initializes a random wrapper using the provided <see cref="Random" /> <paramref name="instance" />.
	/// </summary>
	/// <param name="instance"></param>
	public RandomWrapper(Random instance)
	{
		_instance = instance;
	}

	#region IRandom Members

	/// <inheritdoc cref="IRandom.Next()" />
	public int Next()
		=> _instance.Next();

	/// <inheritdoc cref="IRandom.Next(int)" />
	public int Next(int maxValue)
		=> _instance.Next(maxValue);

	/// <inheritdoc cref="IRandom.Next(int, int)" />
	public int Next(int minValue, int maxValue)
		=> _instance.Next(minValue, maxValue);

	/// <inheritdoc cref="IRandom.NextBytes(byte[])" />
	public void NextBytes(byte[] buffer)
		=> _instance.NextBytes(buffer);

#if FEATURE_SPAN
	/// <inheritdoc cref="IRandom.NextBytes(Span{byte})" />
	public void NextBytes(Span<byte> buffer)
		=> _instance.NextBytes(buffer);
#endif

	/// <inheritdoc cref="IRandom.NextDouble()" />
	public double NextDouble()
		=> _instance.NextDouble();

#if FEATURE_RANDOM_ADVANCED
	/// <inheritdoc cref="IRandom.NextInt64()" />
	public long NextInt64()
		=> _instance.NextInt64();

	/// <inheritdoc cref="IRandom.NextInt64(long)" />
	public long NextInt64(long maxValue)
		=> _instance.NextInt64(maxValue);

	/// <inheritdoc cref="IRandom.NextInt64(long, long)" />
	public long NextInt64(long minValue, long maxValue)
		=> _instance.NextInt64(minValue, maxValue);

	/// <inheritdoc cref="IRandom.NextSingle()" />
	public float NextSingle()
		=> _instance.NextSingle();
#endif

	#endregion
}

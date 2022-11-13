using System;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.Helpers;
using static Testably.Abstractions.Testing.RandomProvider;

namespace Testably.Abstractions.Testing.RandomSystem;

/// <summary>
///     A mocked random generator.
/// </summary>
internal sealed class RandomMock : IRandom
{
	private readonly Generator<byte[]>? _byteGenerator;
	private readonly Generator<double>? _doubleGenerator;
	private readonly Generator<int>? _intGenerator;
	private readonly IRandom _random;

#if FEATURE_RANDOM_ADVANCED
	/// <summary>
	///     Initializes a new instance of <see cref="RandomMock" /> which allows specifying explicit generators:<br />
	///     - <paramref name="intGenerator" />: The generator for <c>int</c> values.
	///     - <paramref name="longGenerator" />: The generator for <c>long</c> values.
	///     - <paramref name="singleGenerator" />: The generator for <c>float</c> values.
	///     - <paramref name="doubleGenerator" />: The generator for <c>double</c> values.
	///     - <paramref name="byteGenerator" />: The generator for <c>byte[]</c> values.
	/// </summary>
#else
		/// <summary>
		///	 Initializes a new instance of <see cref="RandomMock" /> which allows specifying explicit generators:<br />
		///	 - <paramref name="intGenerator" />: The generator for <c>int</c> values.
		///	 - <paramref name="doubleGenerator" />: The generator for <c>double</c> values.
		///	 - <paramref name="byteGenerator" />: The generator for <c>byte[]</c> values.
		/// </summary>
#endif
	public RandomMock(
		int seed = SharedSeed,
		Generator<int>? intGenerator = null,
#if FEATURE_RANDOM_ADVANCED
		Generator<long>? longGenerator = null,
		Generator<float>? singleGenerator = null,
#endif
		Generator<double>? doubleGenerator = null,
		Generator<byte[]>? byteGenerator = null)
	{
		_byteGenerator = byteGenerator;
		_doubleGenerator = doubleGenerator;
		_intGenerator = intGenerator;
#if FEATURE_RANDOM_ADVANCED
		_longGenerator = longGenerator;
		_singleGenerator = singleGenerator;
#endif
		if (seed != SharedSeed)
		{
			_random = new RandomWrapper(new Random(seed));
		}
		else
		{
			_random = RandomFactory.Shared;
		}
	}

	#region IRandom Members

	/// <inheritdoc cref="IRandom.Next()" />
	public int Next()
		=> _intGenerator?.GetNext() ?? _random.Next();

	/// <inheritdoc cref="IRandom.Next(int)" />
	public int Next(int maxValue)
		=> Math.Min(
			_intGenerator?.GetNext() ?? _random.Next(maxValue),
			maxValue - 1);

	/// <inheritdoc cref="IRandom.Next(int, int)" />
	public int Next(int minValue, int maxValue)
		=> Math.Min(
			Math.Max(
				_intGenerator?.GetNext() ?? _random.Next(minValue, maxValue),
				minValue),
			maxValue - 1);

	/// <inheritdoc cref="IRandom.NextBytes(byte[])" />
	public void NextBytes(byte[] buffer)
	{
		if (_byteGenerator != null)
		{
			byte[] bytes = _byteGenerator.GetNext();
			Array.Copy(bytes, buffer, Math.Min(bytes.Length, buffer.Length));
		}
		else
		{
			_random.NextBytes(buffer);
		}
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="IRandom.NextBytes(Span{byte})" />
	public void NextBytes(Span<byte> buffer)
	{
		if (_byteGenerator != null)
		{
			byte[] bytes = _byteGenerator.GetNext();
			bytes.AsSpan().Slice(0, Math.Min(bytes.Length, buffer.Length))
				.CopyTo(buffer);
		}
		else
		{
			_random.NextBytes(buffer);
		}
	}
#endif

	/// <inheritdoc cref="IRandom.NextDouble()" />
	public double NextDouble()
		=> _doubleGenerator?.GetNext() ?? _random.NextDouble();

	#endregion

#if FEATURE_RANDOM_ADVANCED
	private readonly Generator<long>? _longGenerator;
	private readonly Generator<float>? _singleGenerator;
#endif

#if FEATURE_RANDOM_ADVANCED
	/// <inheritdoc cref="IRandom.NextInt64()" />
	public long NextInt64()
		=> _longGenerator?.GetNext() ?? _random.NextInt64();

	/// <inheritdoc cref="IRandom.NextInt64(long)" />
	public long NextInt64(long maxValue)
		=> Math.Min(
			_longGenerator?.GetNext() ?? _random.NextInt64(maxValue),
			maxValue - 1);

	/// <inheritdoc cref="IRandom.NextInt64(long, long)" />
	public long NextInt64(long minValue, long maxValue)
		=> Math.Min(
			Math.Max(
				_longGenerator?.GetNext() ?? _random.NextInt64(minValue, maxValue),
				minValue),
			maxValue - 1);

	/// <inheritdoc cref="IRandom.NextSingle()" />
	public float NextSingle()
		=> _singleGenerator?.GetNext() ?? _random.NextSingle();
#endif
}
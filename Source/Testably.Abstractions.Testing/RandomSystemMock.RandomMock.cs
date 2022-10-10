using System;
using static Testably.Abstractions.Testing.RandomProvider;

namespace Testably.Abstractions.Testing;

public sealed partial class RandomSystemMock
{
	/// <summary>
	///     A mocked random generator.
	/// </summary>
	internal class RandomMock : IRandomSystem.IRandom
	{
		private readonly Generator<byte[]>? _byteGenerator;
		private readonly Generator<double>? _doubleGenerator;
		private readonly Generator<int>? _intGenerator;
		private readonly IRandomSystem.IRandom _random;

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
				_random = new RandomSystem().Random.New(seed);
			}
			else
			{
				_random = new RandomSystem().Random.Shared;
			}
		}

		#region IRandom Members

		/// <inheritdoc cref="IRandomSystem.IRandom.Next()" />
		public int Next()
			=> _intGenerator?.GetNext() ?? _random.Next();

		/// <inheritdoc cref="IRandomSystem.IRandom.Next(int)" />
		public int Next(int maxValue)
			=> Math.Min(
				_intGenerator?.GetNext() ?? _random.Next(maxValue),
				maxValue - 1);

		/// <inheritdoc cref="IRandomSystem.IRandom.Next(int, int)" />
		public int Next(int minValue, int maxValue)
			=> Math.Min(
				Math.Max(
					_intGenerator?.GetNext() ?? _random.Next(minValue, maxValue),
					minValue),
				maxValue - 1);

		/// <inheritdoc cref="IRandomSystem.IRandom.NextBytes(byte[])" />
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
		/// <inheritdoc cref="IRandomSystem.IRandom.NextBytes(Span{byte})" />
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

		/// <inheritdoc cref="IRandomSystem.IRandom.NextDouble()" />
		public double NextDouble()
			=> _doubleGenerator?.GetNext() ?? _random.NextDouble();

		#endregion

#if FEATURE_RANDOM_ADVANCED
		private readonly Generator<long>? _longGenerator;
		private readonly Generator<float>? _singleGenerator;
#endif

#if FEATURE_RANDOM_ADVANCED
		/// <inheritdoc cref="IRandomSystem.IRandom.NextInt64()" />
		public long NextInt64()
			=> _longGenerator?.GetNext() ?? _random.NextInt64();

		/// <inheritdoc cref="IRandomSystem.IRandom.NextInt64(long)" />
		public long NextInt64(long maxValue)
			=> Math.Min(
				_longGenerator?.GetNext() ?? _random.NextInt64(maxValue),
				maxValue - 1);

		/// <inheritdoc cref="IRandomSystem.IRandom.NextInt64(long, long)" />
		public long NextInt64(long minValue, long maxValue)
			=> Math.Min(
				Math.Max(
					_longGenerator?.GetNext() ?? _random.NextInt64(minValue, maxValue),
					minValue),
				maxValue - 1);

		/// <inheritdoc cref="IRandomSystem.IRandom.NextSingle()" />
		public float NextSingle()
			=> _singleGenerator?.GetNext() ?? _random.NextSingle();
#endif
	}
}
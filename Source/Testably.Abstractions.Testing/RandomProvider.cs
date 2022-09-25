using System;
using System.Threading;

namespace Testably.Abstractions.Testing;

/// <summary>
///     <see cref="RandomSystemMock.IRandomProvider" />s for use in the constructor of <see cref="RandomSystemMock" />.
/// </summary>
public static class RandomProvider
{
    /// <summary>
    ///     The internal seed used to indicate a shared random instance.
    /// </summary>
    internal const int SharedSeed = -1;

    private static int _currentSeed = 1;

    /// <summary>
    ///     The default implementation for a random provider.
    /// </summary>
    public static RandomSystemMock.IRandomProvider Default()
        => new RandomProviderImplementation();

    /// <summary>
    ///     Initializes the <see cref="RandomSystemMock.RandomProvider" /> with an explicit <see cref="Guid" /> generator.
    /// </summary>
    public static RandomSystemMock.IRandomProvider GenerateGuid(Func<Guid> guidGenerator)
        => new RandomProviderImplementation(guidGenerator: guidGenerator);

    /// <summary>
    ///     Initializes the <see cref="RandomSystemMock.RandomProvider" /> with explicit generators.
    /// </summary>
    public static RandomSystemMock.IRandomProvider Generate(
        int seed = SharedSeed,
        Func<Guid>? guidGenerator = null,
        Func<int>? intGenerator = null,
#if FEATURE_RANDOM_ADVANCED
        Func<long>? longGenerator = null,
        Func<float>? singleGenerator = null,
#endif
        Func<double>? doubleGenerator = null,
        Func<byte[]>? byteGenerator = null)
        => new RandomProviderImplementation(
            guidGenerator,
            _ => new RandomGenerator(
                seed,
                intGenerator,
#if FEATURE_RANDOM_ADVANCED
                longGenerator,
                singleGenerator,
#endif
                doubleGenerator,
                byteGenerator));

    /// <summary>
    ///     Initializes the <see cref="RandomSystemMock.RandomProvider" /> with explicit generators.
    /// </summary>
    public static RandomSystemMock.IRandomProvider Generate(
        Func<Guid>? guidGenerator = null,
        Func<int, IRandomSystem.IRandom>? randomGenerator = null)
        => new RandomProviderImplementation(
            guidGenerator,
            randomGenerator);

    /// <summary>
    ///     Returns the next seed used when creating a new Random instance without seed.
    /// </summary>
    internal static int NewSeed()
    {
        return Interlocked.Increment(ref _currentSeed);
    }

    /// <summary>
    ///     A random generator.
    /// </summary>
    public class RandomGenerator : IRandomSystem.IRandom
    {
        private readonly Func<int>? _intGenerator;
#if FEATURE_RANDOM_ADVANCED
        private readonly Func<long>? _longGenerator;
        private readonly Func<float>? _singleGenerator;
#endif
        private readonly Func<double>? _doubleGenerator;
        private readonly Func<byte[]>? _byteGenerator;
        private readonly IRandomSystem.IRandom _random;

#if FEATURE_RANDOM_ADVANCED
        /// <summary>
        ///     Initializes a new instance of <see cref="RandomGenerator" /> which allows specifying explicit generators:<br />
        ///     - <paramref name="intGenerator" />: The generator for <c>int</c> values.
        ///     - <paramref name="longGenerator" />: The generator for <c>long</c> values.
        ///     - <paramref name="singleGenerator" />: The generator for <c>float</c> values.
        ///     - <paramref name="doubleGenerator" />: The generator for <c>double</c> values.
        ///     - <paramref name="byteGenerator" />: The generator for <c>byte[]</c> values.
        /// </summary>
#else
        /// <summary>
        ///     Initializes a new instance of <see cref="RandomGenerator" /> which allows specifying explicit generators:<br />
        ///     - <paramref name="intGenerator" />: The generator for <c>int</c> values.
        ///     - <paramref name="doubleGenerator" />: The generator for <c>double</c> values.
        ///     - <paramref name="byteGenerator" />: The generator for <c>byte[]</c> values.
        /// </summary>
#endif
        public RandomGenerator(
            int seed = SharedSeed,
            Func<int>? intGenerator = null,
#if FEATURE_RANDOM_ADVANCED
            Func<long>? longGenerator = null,
            Func<float>? singleGenerator = null,
#endif
            Func<double>? doubleGenerator = null,
            Func<byte[]>? byteGenerator = null)
        {
            _intGenerator = intGenerator;
#if FEATURE_RANDOM_ADVANCED
            _longGenerator = longGenerator;
            _singleGenerator = singleGenerator;
#endif
            _doubleGenerator = doubleGenerator;
            _byteGenerator = byteGenerator;
            if (seed != SharedSeed)
            {
                _random = new RandomSystem().Random.New(seed);
            }
            else
            {
                _random = new RandomSystem().Random.Shared;
            }
        }

        /// <inheritdoc cref="IRandomSystem.IRandom.Next()" />
        public int Next()
            => _intGenerator?.Invoke() ?? _random.Next();

        /// <inheritdoc cref="IRandomSystem.IRandom.Next(int)" />
        public int Next(int maxValue)
            => Math.Min(
                _intGenerator?.Invoke() ?? _random.Next(maxValue),
                maxValue - 1);

        /// <inheritdoc cref="IRandomSystem.IRandom.Next(int, int)" />
        public int Next(int minValue, int maxValue)
            => Math.Min(
                Math.Max(
                    _intGenerator?.Invoke() ?? _random.Next(minValue, maxValue),
                    minValue),
                maxValue - 1);

        /// <inheritdoc cref="IRandomSystem.IRandom.NextBytes(byte[])" />
        public void NextBytes(byte[] buffer)
        {
            if (_byteGenerator != null)
            {
                byte[] bytes = _byteGenerator.Invoke();
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
                byte[] bytes = _byteGenerator.Invoke();
                bytes.AsSpan().Slice(0, buffer.Length).CopyTo(buffer);
            }
            else
            {
                _random.NextBytes(buffer);
            }
        }
#endif

        /// <inheritdoc cref="IRandomSystem.IRandom.NextDouble()" />
        public double NextDouble()
            => _doubleGenerator?.Invoke() ?? _random.NextDouble();

#if FEATURE_RANDOM_ADVANCED
        /// <inheritdoc cref="IRandomSystem.IRandom.NextInt64()" />
        public long NextInt64()
            => _longGenerator?.Invoke() ?? _random.NextInt64();

        /// <inheritdoc cref="IRandomSystem.IRandom.NextInt64(long)" />
        public long NextInt64(long maxValue)
            => Math.Min(
                _longGenerator?.Invoke() ?? _random.NextInt64(maxValue),
                maxValue - 1);

        /// <inheritdoc cref="IRandomSystem.IRandom.NextInt64(long, long)" />
        public long NextInt64(long minValue, long maxValue)
            => Math.Min(
                Math.Max(
                    _longGenerator?.Invoke() ?? _random.NextInt64(minValue, maxValue),
                    minValue),
                maxValue - 1);

        /// <inheritdoc cref="IRandomSystem.IRandom.NextSingle()" />
        public float NextSingle()
            => _singleGenerator?.Invoke() ?? _random.NextSingle();
#endif
    }

    private sealed class RandomProviderImplementation : RandomSystemMock.IRandomProvider
    {
        private readonly Func<Guid> _guidGenerator;
        private readonly Func<int, IRandomSystem.IRandom> _randomGenerator;

        public RandomProviderImplementation(
            Func<Guid>? guidGenerator = null,
            Func<int, IRandomSystem.IRandom>? randomGenerator = null)
        {
            _guidGenerator = guidGenerator ?? DefaultGuidGenerator;
            _randomGenerator = randomGenerator ?? DefaultRandomGenerator;
        }

        private Guid DefaultGuidGenerator()
            => Guid.NewGuid();

        private IRandomSystem.IRandom DefaultRandomGenerator(int seed)
            => new RandomGenerator(seed: seed);

        #region IRandomProvider Members

        /// <inheritdoc cref="RandomSystemMock.IRandomProvider.GetGuid" />
        public Guid GetGuid()
            => _guidGenerator();

        /// <inheritdoc cref="RandomSystemMock.IRandomProvider.GetRandom(int)" />
        public IRandomSystem.IRandom GetRandom(int seed)
            => _randomGenerator(seed);

        #endregion
    }
}
using System;
using System.Collections.Generic;
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
    ///     Initializes the <see cref="RandomSystemMock.RandomProvider" /> with explicit generators.
    /// </summary>
    public static RandomSystemMock.IRandomProvider Generate(
        int seed = SharedSeed,
        Generator<Guid>? guidGenerator = null,
        Generator<int>? intGenerator = null,
#if FEATURE_RANDOM_ADVANCED
        Generator<long>? longGenerator = null,
        Generator<float>? singleGenerator = null,
#endif
        Generator<double>? doubleGenerator = null,
        Generator<byte[]>? byteGenerator = null)
        => new RandomProviderImplementation(
            _ => new RandomGenerator(
                seed,
                intGenerator,
#if FEATURE_RANDOM_ADVANCED
                longGenerator,
                singleGenerator,
#endif
                doubleGenerator,
                byteGenerator),
            guidGenerator);

    /// <summary>
    ///     Initializes the <see cref="RandomSystemMock.RandomProvider" /> with explicit generators.
    /// </summary>
    public static RandomSystemMock.IRandomProvider Generate(
        Func<int, IRandomSystem.IRandom>? randomGenerator,
        Generator<Guid>? guidGenerator = null)
        => new RandomProviderImplementation(
            randomGenerator,
            guidGenerator);

    /// <summary>
    ///     Returns the next seed used when creating a new Random instance without seed.
    /// </summary>
    internal static int NewSeed()
    {
        return Interlocked.Increment(ref _currentSeed);
    }

    /// <summary>
    ///     Replaces random element generation of type <typeparamref name="T" />.
    /// </summary>
    public sealed class Generator<T> : IDisposable
    {
        private readonly IDisposable? _disposable;
        private readonly Func<T> _getNextCallback;

        private Generator(Func<T> getNextCallback, IDisposable? disposable = null)
        {
            _getNextCallback = getNextCallback;
            _disposable = disposable;
        }

        #region IDisposable Members

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose() => _disposable?.Dispose();

        #endregion

        /// <summary>
        ///     Creates a generator the iterates over the provided <paramref name="values" />.
        ///     <para />
        ///     When the end of the array is reached, the values are repeated again from the beginning.
        /// </summary>
        public static Generator<T> FromArray(T[] values)
        {
            int index = 0;
            return new Generator<T>(() => values[index++ % values.Length]);
        }

        /// <summary>
        ///     Creates a generator that gets the elements from the provided <paramref name="callback" />
        /// </summary>
        public static Generator<T> FromDelegate(Func<T> callback)
        {
            return new Generator<T>(callback);
        }

        /// <summary>
        ///     Creates a generator the iterates over the provided <paramref name="enumerable" />.
        /// </summary>
        public static Generator<T> FromEnumerable(IEnumerable<T> enumerable)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            return new Generator<T>(() =>
            {
                T result = enumerator.Current;
                enumerator.MoveNext();
                return result;
            }, enumerator);
        }

        /// <summary>
        ///     Creates a generator that always returns the fixed <paramref name="value" />.
        /// </summary>
        public static Generator<T> FromValue(T value)
        {
            return new Generator<T>(() => value);
        }

        /// <summary>
        ///     Gets the next value of <typeparamref name="T" />
        /// </summary>
        public T GetNext()
        {
            return _getNextCallback();
        }

        /// <summary>
        ///     Implicit operator to convert from a <see cref="Func{T}" /> to a <see cref="Generator{T}" />.
        /// </summary>
        public static implicit operator Generator<T>(Func<T> callback)
            => FromDelegate(callback);

        /// <summary>
        ///     Implicit operator to convert from an array of <typeparamref name="T" /> to a <see cref="Generator{T}" />.
        /// </summary>
        public static implicit operator Generator<T>(T[] values)
            => FromArray(values);

        /// <summary>
        ///     Implicit operator to convert from a fixed <paramref name="value" /> of <typeparamref name="T" /> to a
        ///     <see cref="Generator{T}" />.
        /// </summary>
        public static implicit operator Generator<T>(T value)
            => FromValue(value);
    }

    /// <summary>
    ///     A random generator.
    /// </summary>
    public class RandomGenerator : IRandomSystem.IRandom
    {
        private readonly Generator<byte[]>? _byteGenerator;
        private readonly Generator<double>? _doubleGenerator;
        private readonly Generator<int>? _intGenerator;
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

    private sealed class RandomProviderImplementation : RandomSystemMock.IRandomProvider
    {
        private Generator<Guid> DefaultGuidGenerator
            => Generator<Guid>.FromDelegate(Guid.NewGuid);

        private readonly Generator<Guid> _guidGenerator;
        private readonly Func<int, IRandomSystem.IRandom> _randomGenerator;

        public RandomProviderImplementation(
            Func<int, IRandomSystem.IRandom>? randomGenerator = null,
            Generator<Guid>? guidGenerator = null)
        {
            _guidGenerator = guidGenerator ?? DefaultGuidGenerator;
            _randomGenerator = randomGenerator ?? DefaultRandomGenerator;
        }

        #region IRandomProvider Members

        /// <inheritdoc cref="RandomSystemMock.IRandomProvider.GetGuid" />
        public Guid GetGuid()
            => _guidGenerator.GetNext();

        /// <inheritdoc cref="RandomSystemMock.IRandomProvider.GetRandom(int)" />
        public IRandomSystem.IRandom GetRandom(int seed)
            => _randomGenerator(seed);

        #endregion

        private IRandomSystem.IRandom DefaultRandomGenerator(int seed)
            => new RandomGenerator(seed: seed);
    }
}
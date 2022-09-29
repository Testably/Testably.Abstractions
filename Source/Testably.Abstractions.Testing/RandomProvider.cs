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
        => new RandomSystemMock.RandomProviderMock();

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
        => new RandomSystemMock.RandomProviderMock(
            _ => new RandomSystemMock.RandomMock(
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
        => new RandomSystemMock.RandomProviderMock(
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
        private readonly Func<T> _callback;
        private readonly IEnumerator<T>? _enumerator;
        private bool _isDisposed;

        private Generator(Func<T> callback)
        {
            _callback = callback;
        }

        private Generator(IEnumerable<T> enumerable)
        {
            _enumerator = enumerable.GetEnumerator();
            _callback = () =>
            {
                if (!_enumerator.MoveNext())
                {
                    _enumerator.Reset();
                    _enumerator.MoveNext();
                }

                return _enumerator.Current;
            };
        }

        #region IDisposable Members

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
        {
            _isDisposed = true;
            _enumerator?.Dispose();
        }

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
        public static Generator<T> FromCallback(Func<T> callback)
        {
            return new Generator<T>(callback);
        }

        /// <summary>
        ///     Creates a generator the iterates over the provided <paramref name="enumerable" />.
        /// </summary>
        public static Generator<T> FromEnumerable(IEnumerable<T> enumerable)
        {
            return new Generator<T>(enumerable);
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
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(Generator<T>));
            }

            return _callback();
        }

        /// <summary>
        ///     Implicit operator to convert from a <see cref="Func{T}" /> to a <see cref="Generator{T}" />.
        /// </summary>
        public static implicit operator Generator<T>(Func<T> callback)
            => FromCallback(callback);

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
}
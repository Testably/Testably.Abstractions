using System;

namespace Testably.Abstractions;

public sealed partial class RandomSystem
{
    private sealed class RandomFactory : IRandomSystem.IRandomFactory
    {
        private IRandomSystem.IRandom? _shared;

        private static readonly Random Global = new();

        internal RandomFactory(RandomSystem timeSystem)
        {
            RandomSystem = timeSystem;
        }

        #region IRandomFactory Members

        /// <inheritdoc cref="IRandomSystem.IRandomSystemExtensionPoint.RandomSystem" />
        public IRandomSystem RandomSystem { get; }

        #endregion

        /// <inheritdoc cref="IRandomSystem.IRandomFactory.Shared" />
        public IRandomSystem.IRandom Shared
        {
            get
            {
                if (_shared is null)
                {
                    _shared = CreateThreadSafeRandomWrapper();
                }

                return _shared;
            }
        }

        /// <inheritdoc cref="IRandomSystem.IRandomFactory.New()" />
        public IRandomSystem.IRandom New()
            => CreateThreadSafeRandomWrapper();

        /// <inheritdoc cref="IRandomSystem.IRandomFactory.New(int)" />
        public IRandomSystem.IRandom New(int seed)
            => new RandomWrapper(new Random(seed));

        /// <summary>
        ///     <see href="https://andrewlock.net/building-a-thread-safe-random-implementation-for-dotnet-framework/" />
        /// </summary>
        private static IRandomSystem.IRandom CreateThreadSafeRandomWrapper()
        {
            int seed;
            lock (Global)
            {
                seed = Global.Next();
            }

            return new RandomWrapper(new Random(seed));
        }

        private sealed class RandomWrapper : IRandomSystem.IRandom
        {
            private readonly Random _instance;

            public RandomWrapper(Random instance)
            {
                _instance = instance;
            }

            /// <inheritdoc cref="IRandomSystem.IRandom.Next()" />
            public int Next()
                => _instance.Next();

            /// <inheritdoc cref="IRandomSystem.IRandom.Next(int)" />
            public int Next(int maxValue)
                => _instance.Next(maxValue);

            /// <inheritdoc cref="IRandomSystem.IRandom.Next(int, int)" />
            public int Next(int minValue, int maxValue)
                => _instance.Next(minValue, maxValue);

            /// <inheritdoc cref="IRandomSystem.IRandom.NextBytes(byte[])" />
            public void NextBytes(byte[] buffer)
                => _instance.NextBytes(buffer);

#if FEATURE_SPAN
            /// <inheritdoc cref="IRandomSystem.IRandom.NextBytes(Span{byte})" />
            public void NextBytes(Span<byte> buffer)
                => _instance.NextBytes(buffer);
#endif

            /// <inheritdoc cref="IRandomSystem.IRandom.NextDouble()" />
            public double NextDouble()
                => _instance.NextDouble();

#if FEATURE_RANDOM_ADVANCED
            /// <inheritdoc cref="IRandomSystem.IRandom.NextInt64()" />
            public long NextInt64()
                => _instance.NextInt64();

            /// <inheritdoc cref="IRandomSystem.IRandom.NextInt64(long)" />
            public long NextInt64(long maxValue)
                => _instance.NextInt64(maxValue);

            /// <inheritdoc cref="IRandomSystem.IRandom.NextInt64(long, long)" />
            public long NextInt64(long minValue, long maxValue)
                => _instance.NextInt64(minValue, maxValue);

            /// <inheritdoc cref="IRandomSystem.IRandom.NextSingle()" />
            public float NextSingle()
                => _instance.NextSingle();
#endif
        }
    }
}
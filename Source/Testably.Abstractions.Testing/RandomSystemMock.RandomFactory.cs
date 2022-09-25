using System;

namespace Testably.Abstractions.Testing;

public sealed partial class RandomSystemMock
{
    private sealed class RandomFactoryMock : IRandomSystem.IRandomFactory
    {
        private readonly RandomSystemMock _randomSystemMock;

        internal RandomFactoryMock(RandomSystemMock randomSystem)
        {
            _randomSystemMock = randomSystem;
        }

        #region IRandomFactory Members

        /// <inheritdoc cref="IRandomSystem.IRandomSystemExtensionPoint.RandomSystem" />
        public IRandomSystem RandomSystem => _randomSystemMock;

        /// <inheritdoc cref="IRandomSystem.IRandomFactory.Shared" />
        public IRandomSystem.IRandom Shared
            => _randomSystemMock.RandomProvider.GetRandom(Testing.RandomProvider.SharedSeed);

        /// <inheritdoc cref="IRandomSystem.IRandomFactory.New()" />
        public IRandomSystem.IRandom New()
            => _randomSystemMock.RandomProvider.GetRandom(Testing.RandomProvider.NewSeed());

        /// <inheritdoc cref="IRandomSystem.IRandomFactory.New(int)" />
        public IRandomSystem.IRandom New(int seed)
            => _randomSystemMock.RandomProvider.GetRandom(seed);

        #endregion
    }
}
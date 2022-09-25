using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing;

public sealed partial class RandomSystemMock
{
    private sealed class GuidMock : GuidSystem
    {
        private readonly RandomSystemMock _randomSystemMock;

        internal GuidMock(RandomSystemMock randomSystem) : base(randomSystem)
        {
            _randomSystemMock = randomSystem;
        }

        /// <inheritdoc cref="IRandomSystem.IGuid.NewGuid()" />
        public override Guid NewGuid()
            => _randomSystemMock.RandomProvider.GetGuid();
    }
}
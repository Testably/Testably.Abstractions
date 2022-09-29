using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

public sealed partial class RandomSystem
{
    private sealed class GuidRandomSystem : GuidSystemBase
    {
        internal GuidRandomSystem(RandomSystem randomSystem) : base(randomSystem)
        {
        }

        /// <inheritdoc cref="IRandomSystem.IGuid.NewGuid()" />
        public override Guid NewGuid()
            => System.Guid.NewGuid();
    }
}
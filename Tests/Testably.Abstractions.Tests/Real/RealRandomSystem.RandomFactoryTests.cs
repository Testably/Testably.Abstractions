namespace Testably.Abstractions.Tests.Real;

public static partial class RealRandomSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RandomFactoryTests : RandomSystemRandomFactoryTests<RandomSystem>
    {
        public RandomFactoryTests() : base(new RandomSystem())
        {
        }
    }
}
namespace Testably.Abstractions.Tests.Real;

public static partial class RealRandomSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RandomTests : RandomSystemRandomTests<RandomSystem>
    {
        public RandomTests() : base(new RandomSystem())
        {
        }
    }
}
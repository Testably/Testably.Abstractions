namespace Testably.Abstractions.Tests.Real;

public static partial class RealRandomSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Tests : RandomSystemTests<RandomSystem>
    {
        public Tests() : base(new RandomSystem())
        {
        }
    }
}
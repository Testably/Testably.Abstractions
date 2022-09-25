namespace Testably.Abstractions.Tests.Mock;

public static partial class MockRandomSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Tests : RandomSystemTests<RandomSystemMock>
    {
        public Tests() : base(new RandomSystemMock())
        {
        }
    }
}
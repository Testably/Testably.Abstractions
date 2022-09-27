namespace Testably.Abstractions.Tests;

public abstract class RandomSystemTests<TRandomSystem>
    where TRandomSystem : IRandomSystem
{
    #region Test Setup

    public TRandomSystem RandomSystem { get; }

    protected RandomSystemTests(TRandomSystem randomSystem)
    {
        RandomSystem = randomSystem;
    }

    #endregion

    [Fact]
    public void Guid_ShouldSetExtensionPoint()
    {
        IRandomSystem result = RandomSystem.Guid.RandomSystem;

        result.Should().Be(RandomSystem);
    }

    [Fact]
    public void Random_ShouldSetExtensionPoint()
    {
        IRandomSystem result = RandomSystem.Random.RandomSystem;

        result.Should().Be(RandomSystem);
    }
}
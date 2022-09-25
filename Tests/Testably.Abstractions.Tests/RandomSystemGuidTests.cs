using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract class RandomSystemGuidTests<TRandomSystem>
    where TRandomSystem : IRandomSystem
{
    #region Test Setup

    public TRandomSystem RandomSystem { get; }

    protected RandomSystemGuidTests(TRandomSystem randomSystem)
    {
        RandomSystem = randomSystem;
    }

    #endregion

    [Fact]
    public void NewGuid_ShouldBeThreadSafeAndReturnUniqueItems()
    {
        ConcurrentBag<Guid> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Guid.NewGuid());
        });

        results.Should().OnlyHaveUniqueItems();
    }
}
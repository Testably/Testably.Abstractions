using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract class RandomSystemRandomTests<TRandomSystem>
    where TRandomSystem : IRandomSystem
{
    #region Test Setup

    public TRandomSystem RandomSystem { get; }

    protected RandomSystemRandomTests(TRandomSystem randomSystem)
    {
        RandomSystem = randomSystem;
    }

    #endregion

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.Next))]
    public void Next_MaxValue_ShouldOnlyReturnValidValues()
    {
        int maxValue = 10;
        ConcurrentBag<int> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.Next(maxValue));
        });

        results.Should().OnlyContain(r => r < maxValue);
    }

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.Next))]
    public void Next_MinAndMaxValue_ShouldOnlyReturnValidValues()
    {
        int minValue = 10;
        int maxValue = 20;
        ConcurrentBag<int> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.Next(minValue, maxValue));
        });

        results.Should().OnlyContain(r => r >= minValue && r < maxValue);
    }

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.Next))]
    public void Next_ShouldBeThreadSafe()
    {
        ConcurrentBag<int> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.Next());
        });

        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.NextBytes))]
    public void NextBytes_ShouldBeThreadSafe()
    {
        ConcurrentBag<byte[]> results = new();

        Parallel.For(0, 100, _ =>
        {
            byte[] bytes = new byte[100];
            RandomSystem.Random.Shared.NextBytes(bytes);
            results.Add(bytes);
        });

        results.Should().OnlyHaveUniqueItems();
    }

#if FEATURE_SPAN
    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.NextBytes))]
    public void NextBytes_Span_ShouldBeThreadSafe()
    {
        ConcurrentBag<byte[]> results = new();

        Parallel.For(0, 100, _ =>
        {
            Span<byte> bytes = new byte[100];
            RandomSystem.Random.Shared.NextBytes(bytes);
            results.Add(bytes.ToArray());
        });

        results.Should().OnlyHaveUniqueItems();
    }
#endif

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.NextDouble))]
    public void NextDouble_ShouldBeThreadSafe()
    {
        ConcurrentBag<double> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.NextDouble());
        });

        results.Should().OnlyHaveUniqueItems();
    }

#if FEATURE_RANDOM_ADVANCED
    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.NextInt64))]
    public void NextInt64_MaxValue_ShouldOnlyReturnValidValues()
    {
        long maxValue = 10;
        ConcurrentBag<long> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.NextInt64(maxValue));
        });

        results.Should().OnlyContain(r => r < maxValue);
    }

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.NextInt64))]
    public void NextInt64_MinAndMaxValue_ShouldOnlyReturnValidValues()
    {
        long minValue = 10;
        long maxValue = 20;
        ConcurrentBag<long> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.NextInt64(minValue, maxValue));
        });

        results.Should().OnlyContain(r => r >= minValue && r < maxValue);
    }

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.NextInt64))]
    public void NextInt64_ShouldBeThreadSafe()
    {
        ConcurrentBag<long> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.NextInt64());
        });

        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [RandomSystemTests.Random(nameof(IRandomSystem.IRandom.NextSingle))]
    public void NextSingle_ShouldBeThreadSafe()
    {
        ConcurrentBag<float> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.NextSingle());
        });

        results.Should().OnlyHaveUniqueItems();
    }
#endif
}
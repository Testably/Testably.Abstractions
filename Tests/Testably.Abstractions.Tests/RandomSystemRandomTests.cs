using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public void New_Next_ShouldReturnDifferentValues()
    {
        List<int> results = new();

        for (int i = 0; i < 100; i++)
        {
            results.Add(RandomSystem.Random.New().Next());
        }

        results.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [AutoData]
    public void New_Next_WithSeed_ShouldReturnSameValue(int seed)
    {
        List<int> results = new();

        for (int i = 0; i < 100; i++)
        {
            results.Add(RandomSystem.Random.New(seed).Next());
        }

        results.Should().AllBeEquivalentTo(results.First());
    }

    [Fact]
    public void Shared_Next_MaxValue_ShouldOnlyReturnValidValues()
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
    public void Shared_Next_MinAndMaxValue_ShouldOnlyReturnValidValues()
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
    public void Shared_Next_ShouldBeThreadSafe()
    {
        ConcurrentBag<int> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.Next());
        });

        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Shared_NextBytes_ShouldBeThreadSafe()
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
    public void Shared_NextBytes_Span_ShouldBeThreadSafe()
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
    public void Shared_NextDouble_ShouldBeThreadSafe()
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
    public void Shared_NextInt64_MaxValue_ShouldOnlyReturnValidValues()
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
    public void Shared_NextInt64_MinAndMaxValue_ShouldOnlyReturnValidValues()
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
    public void Shared_NextInt64_ShouldBeThreadSafe()
    {
        ConcurrentBag<long> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(RandomSystem.Random.Shared.NextInt64());
        });

        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Shared_NextSingle_ShouldBeThreadSafe()
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
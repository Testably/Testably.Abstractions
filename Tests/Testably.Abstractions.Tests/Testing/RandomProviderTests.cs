using System.Collections.Generic;

namespace Testably.Abstractions.Tests.Testing;

public class RandomProviderTests
{
    [Fact]
    public void Default_ShouldReturnRandomGuid()
    {
        List<Guid> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.Default();

        for (int i = 0; i < 100; i++)
        {
            results.Add(randomProvider.GetGuid());
        }

        results.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [AutoData]
    public void GenerateGuid_ShouldReturnSpecifiedGuid(Guid guid)
    {
        List<Guid> results = new();
        RandomSystemMock.IRandomProvider randomProvider =
            RandomProvider.GenerateGuid(() => guid);

        for (int i = 0; i < 100; i++)
        {
            results.Add(randomProvider.GetGuid());
        }

        results.Should().AllBeEquivalentTo(guid);
    }

    [Theory]
    [AutoData]
    public void GenerateGuid_ShouldReturnSpecifiedGuids(Guid[] guids)
    {
        int index = 0;
        List<Guid> results = new();
        RandomSystemMock.IRandomProvider randomProvider =
            RandomProvider.GenerateGuid(() => guids[index++ % guids.Length]);

        for (int i = 0; i < 100; i++)
        {
            results.Add(randomProvider.GetGuid());
        }

        results.Should().ContainInOrder(guids);
    }

    [Theory]
    [AutoData]
    public void GenerateRandom_Next_ShouldReturnSpecifiedValue(int seed, int value)
    {
        List<int> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(() => value));

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.Next());
        }

        results.Should().AllBeEquivalentTo(value);
    }

    [Theory]
    [AutoData]
    public void GenerateRandom_NextBytes_ShouldReturnSpecifiedValue(int seed, byte[] value)
    {
        List<byte[]> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(byteGenerator: () => value));

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            var buffer = new byte[value.Length];
            random.NextBytes(buffer);
            results.Add(buffer);
        }

        results.Should().AllBeEquivalentTo(value);
    }

#if FEATURE_SPAN
    [Theory]
    [AutoData]
    public void GenerateRandom_NextBytes_Span_ShouldReturnSpecifiedValue(int seed, byte[] value)
    {
        List<byte[]> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(byteGenerator: () => value));

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            var buffer = new byte[value.Length];
            random.NextBytes(buffer.AsSpan());
            results.Add(buffer);
        }

        results.Should().AllBeEquivalentTo(value);
    }
#endif

    [Theory]
    [AutoData]
    public void GenerateRandom_NextDouble_ShouldReturnSpecifiedValue(int seed, double value)
    {
        List<double> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(doubleGenerator: () => value));

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextDouble());
        }

        results.Should().AllBeEquivalentTo(value);
    }

#if FEATURE_RANDOM_ADVANCED
    [Theory]
    [AutoData]
    public void GenerateRandom_NextSingle_ShouldReturnSpecifiedValue(int seed, float value)
    {
        List<float> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(singleGenerator: () => value));

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextSingle());
        }

        results.Should().AllBeEquivalentTo(value);
    }
#endif

    [Theory]
    [AutoData]
    public void GenerateRandom_Next_WithMaxValue_ShouldReturnSpecifiedValue(
        int seed, int value)
    {
        int maxValue = 10;
        List<int> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(() => value));
        int expectedValue = Math.Min(value, maxValue - 1);

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.Next(maxValue));
        }

        results.Should().AllBeEquivalentTo(expectedValue);
    }

    [Theory]
    [AutoData]
    public void GenerateRandom_Next_WithMinAndMaxValue_ShouldReturnSpecifiedValue(
        int seed, int value)
    {
        int minValue = 10;
        int maxValue = 20;
        List<int> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(() => value));
        int expectedValue = Math.Max(Math.Min(value, maxValue - 1), minValue);

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.Next(minValue, maxValue));
        }

        results.Should().AllBeEquivalentTo(expectedValue);
    }

#if FEATURE_RANDOM_ADVANCED
    [Theory]
    [AutoData]
    public void GenerateRandom_NextInt64_ShouldReturnSpecifiedValue(int seed, long value)
    {
        List<long> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(longGenerator: () => value));

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextInt64());
        }

        results.Should().AllBeEquivalentTo(value);
    }

    [Theory]
    [AutoData]
    public void GenerateRandom_NextInt64_WithMaxValue_ShouldReturnSpecifiedValue(
        int seed, long value)
    {
        int maxValue = 10;
        List<long> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(longGenerator: () => value));
        long expectedValue = Math.Min(value, maxValue - 1);

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextInt64(maxValue));
        }

        results.Should().AllBeEquivalentTo(expectedValue);
    }

    [Theory]
    [AutoData]
    public void GenerateRandom_NextInt64_WithMinAndMaxValue_ShouldReturnSpecifiedValue(
        int seed, long value)
    {
        long minValue = 10;
        long maxValue = 20;
        List<long> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(longGenerator: () => value));
        long expectedValue = Math.Max(Math.Min(value, maxValue - 1), minValue);

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.NextInt64(minValue, maxValue));
        }

        results.Should().AllBeEquivalentTo(expectedValue);
    }
#endif

    [Theory]
    [AutoData]
    public void GenerateRandom_Next_ShouldReturnSpecifiedValues(int seed, int[] values)
    {
        int index = 0;
        List<int> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.GenerateRandom(
            new RandomProvider.RandomGenerator(() => values[index++ % values.Length]));

        IRandomSystem.IRandom random = randomProvider.GetRandom(seed);
        for (int i = 0; i < 100; i++)
        {
            results.Add(random.Next());
        }

        results.Should().ContainInOrder(values);
    }

    [Fact]
    public void Default_ShouldReturnRandomNumbers()
    {
        List<int> results = new();
        RandomSystemMock.IRandomProvider randomProvider = RandomProvider.Default();

        for (int i = 0; i < 100; i++)
        {
            results.Add(randomProvider.GetRandom(i).Next());
        }

        results.Should().OnlyHaveUniqueItems();
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.Tests.RandomProvider;

public class RandomProviderTests
{
	[Fact]
	public void Default_ShouldReturnRandomGuid()
	{
		List<Guid> results = new();
		RandomSystemMock.IRandomProvider
			randomProvider = Testing.RandomProvider.Default();

		for (int i = 0; i < 100; i++)
		{
			results.Add(randomProvider.GetGuid());
		}

		results.Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void Default_ShouldReturnRandomNumbers()
	{
		List<int> results = new();
		RandomSystemMock.IRandomProvider
			randomProvider = Testing.RandomProvider.Default();

		for (int i = 0; i < 100; i++)
		{
			results.Add(randomProvider.GetRandom(i).Next());
		}

		results.Should().OnlyHaveUniqueItems();
	}

	[Theory]
	[AutoData]
	public void GenerateGuid_ShouldReturnSpecifiedGuid(Guid guid)
	{
		List<Guid> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(guidGenerator: guid);

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
		List<Guid> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(guidGenerator: guids);

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
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(seed, intGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next());
		}

		results.Should().AllBeEquivalentTo(value);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_Next_ShouldReturnSpecifiedValues(int seed, int[] values)
	{
		List<int> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(intGenerator: values);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next());
		}

		results.Should().ContainInOrder(values);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_Next_WithMaxValue_ShouldReturnSpecifiedValue(
		int seed, int value)
	{
		int maxValue = value - 1;
		List<int> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(intGenerator: value);
		int expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next(maxValue));
		}

		results.Should().AllBeEquivalentTo(expectedValue);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_Next_WithMinAndMaxValue_Larger_ShouldReturnSpecifiedValue(
		int seed, int value)
	{
		int minValue = value - 10;
		int maxValue = value - 1;
		List<int> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(intGenerator: value);
		int expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next(minValue, maxValue));
		}

		results.Should().AllBeEquivalentTo(expectedValue);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_Next_WithMinAndMaxValue_Smaller_ShouldReturnSpecifiedValue(
		int seed, int value)
	{
		int minValue = value + 1;
		int maxValue = minValue + 10;
		List<int> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(intGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next(minValue, maxValue));
		}

		results.Should().AllBeEquivalentTo(minValue);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_Next_WithoutGenerator_ShouldReturnRandomValues(int seed)
	{
		List<int> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.Next());
		}

		results.Should().OnlyHaveUniqueItems();
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_NextBytes_ShouldReturnSpecifiedValue(
		int seed, byte[] value)
	{
		List<byte[]> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length];
			random.NextBytes(buffer);
			results.Add(buffer);
		}

		results.Should().AllBeEquivalentTo(value);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_NextBytes_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<byte[]> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			byte[] buffer = new byte[10];
			random.NextBytes(buffer);
			results.Add(buffer);
		}

		results.Should().OnlyHaveUniqueItems();
	}

	[Theory]
	[AutoData]
	public void
		GenerateRandom_NextBytes_WithSmallerBuffer_ShouldReturnPartlyInitializedBytes(
			int seed, byte[] value)
	{
		List<byte[]> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length + 1];
			random.NextBytes(buffer);
			results.Add(buffer);
		}

		results.Should().AllBeEquivalentTo(value.Concat(new[] { (byte)0 }));
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_NextDouble_ShouldReturnSpecifiedValue(
		int seed, double value)
	{
		List<double> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(doubleGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextDouble());
		}

		results.Should().AllBeEquivalentTo(value);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_NextDouble_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<double> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.NextDouble());
		}

		results.Should().OnlyHaveUniqueItems();
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void NextBytes_Span_WithoutByteGenerator_ShouldUseRealRandomValuesFromSeed(
		int seed)
	{
		Span<byte> result = new byte[100];
		Random random = new(seed);
		byte[] expectedBytes = new byte[result.Length];
		random.NextBytes(expectedBytes);
		RandomSystemMock.IRandomProvider sut = Testing.RandomProvider.Generate(seed);

		sut.GetRandom().NextBytes(result);

		result.ToArray().Should().BeEquivalentTo(expectedBytes);
	}
#endif

	[Theory]
	[AutoData]
	public void NextBytes_WithoutByteGenerator_ShouldUseRealRandomValuesFromSeed(
		int seed)
	{
		byte[] result = new byte[100];
		Random random = new(seed);
		byte[] expectedBytes = new byte[result.Length];
		random.NextBytes(expectedBytes);
		RandomSystemMock.IRandomProvider sut = Testing.RandomProvider.Generate(seed);

		sut.GetRandom().NextBytes(result);

		result.Should().BeEquivalentTo(expectedBytes);
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void GenerateRandom_NextBytes_Span_ShouldReturnSpecifiedValue(
		int seed, byte[] value)
	{
		List<byte[]> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length];
			random.NextBytes(buffer.AsSpan());
			results.Add(buffer);
		}

		results.Should().AllBeEquivalentTo(value);
	}

	[Theory]
	[AutoData]
	public void
		GenerateRandom_NextBytes_Span_WithSmallerBuffer_ShouldReturnPartlyInitializedBytes(
			int seed, byte[] value)
	{
		List<byte[]> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length + 1];
			random.NextBytes(buffer.AsSpan());
			results.Add(buffer);
		}

		results.Should().AllBeEquivalentTo(value.Concat(new[] { (byte)0 }));
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_NextBytes_Span_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<byte[]> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			byte[] buffer = new byte[10];
			random.NextBytes(buffer.AsSpan());
			results.Add(buffer);
		}

		results.Should().OnlyHaveUniqueItems();
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public void GenerateRandom_NextSingle_ShouldReturnSpecifiedValue(
		int seed, float value)
	{
		List<float> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(singleGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextSingle());
		}

		results.Should().AllBeEquivalentTo(value);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_NextSingle_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<float> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.NextSingle());
		}

		results.Should().OnlyHaveUniqueItems();
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public void GenerateRandom_NextInt64_ShouldReturnSpecifiedValue(int seed, long value)
	{
		List<long> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(longGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
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
		long maxValue = value - 1;
		List<long> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(longGenerator: value);
		long expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextInt64(maxValue));
		}

		results.Should().AllBeEquivalentTo(expectedValue);
	}

	[Theory]
	[AutoData]
	public void
		GenerateRandom_NextInt64_WithMinAndMaxValue_Smaller_ShouldReturnSpecifiedValue(
			int seed, long value)
	{
		long minValue = value + 1;
		long maxValue = minValue + 10;
		List<long> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(longGenerator: value);
		long expectedValue = minValue;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextInt64(minValue, maxValue));
		}

		results.Should().AllBeEquivalentTo(expectedValue);
	}

	[Theory]
	[AutoData]
	public void
		GenerateRandom_NextInt64_WithMinAndMaxValue_Larger_ShouldReturnSpecifiedValue(
			int seed, long value)
	{
		long minValue = value - 10;
		long maxValue = value - 1;
		List<long> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate(longGenerator: value);
		long expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextInt64(minValue, maxValue));
		}

		results.Should().AllBeEquivalentTo(expectedValue);
	}

	[Theory]
	[AutoData]
	public void GenerateRandom_NextInt64_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<long> results = new();
		RandomSystemMock.IRandomProvider randomProvider =
			Testing.RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.NextInt64());
		}

		results.Should().OnlyHaveUniqueItems();
	}
#endif
}
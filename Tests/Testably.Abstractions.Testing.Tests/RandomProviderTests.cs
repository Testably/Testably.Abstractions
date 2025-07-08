using System.Collections.Generic;
using System.Linq;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.RandomSystem;

namespace Testably.Abstractions.Testing.Tests;

public partial class RandomProviderTests
{
	[Fact]
	public async Task Default_ShouldReturnRandomGuid()
	{
		List<Guid> results = [];
		IRandomProvider
			randomProvider = RandomProvider.Default();

		for (int i = 0; i < 100; i++)
		{
			results.Add(randomProvider.GetGuid());
		}

		await That(results).AreAllUnique();
	}

	[Fact]
	public async Task Default_ShouldReturnRandomNumbers()
	{
		List<int> results = [];
		IRandomProvider
			randomProvider = RandomProvider.Default();

		for (int i = 0; i < 100; i++)
		{
			results.Add(randomProvider.GetRandom(i).Next());
		}

		await That(results).AreAllUnique();
	}

	[Theory]
	[AutoData]
	public async Task GenerateGuid_ShouldReturnSpecifiedGuid(Guid guid)
	{
		List<Guid> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(guidGenerator: guid);

		for (int i = 0; i < 100; i++)
		{
			results.Add(randomProvider.GetGuid());
		}

		await That(results).All().AreEqualTo(guid);
	}

	[Theory]
	[AutoData]
	public async Task GenerateGuid_ShouldReturnSpecifiedGuids(Guid[] guids)
	{
		List<Guid> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(guidGenerator: guids);

		for (int i = 0; i < 100; i++)
		{
			results.Add(randomProvider.GetGuid());
		}

		await That(results).Contains(guids).IgnoringInterspersedItems();
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_Next_ShouldReturnSpecifiedValue(int seed, int value)
	{
		List<int> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(seed, intGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next());
		}

		await That(results).All().AreEqualTo(value);
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_Next_ShouldReturnSpecifiedValues(int seed, int[] values)
	{
		List<int> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(intGenerator: values);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next());
		}

		await That(results).Contains(values).IgnoringInterspersedItems();
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_Next_WithMaxValue_ShouldReturnSpecifiedValue(
		int seed, int value)
	{
		int maxValue = value - 1;
		List<int> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(intGenerator: value);
		int expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next(maxValue));
		}

		await That(results).All().AreEqualTo(expectedValue);
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_Next_WithMinAndMaxValue_Larger_ShouldReturnSpecifiedValue(
		int seed, int value)
	{
		int minValue = value - 10;
		int maxValue = value - 1;
		List<int> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(intGenerator: value);
		int expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next(minValue, maxValue));
		}

		await That(results).All().AreEqualTo(expectedValue);
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_Next_WithMinAndMaxValue_Smaller_ShouldReturnSpecifiedValue(
		int seed, int value)
	{
		int minValue = value + 1;
		int maxValue = minValue + 10;
		List<int> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(intGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.Next(minValue, maxValue));
		}

		await That(results).All().AreEqualTo(minValue);
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_Next_WithoutGenerator_ShouldReturnRandomValues(int seed)
	{
		List<int> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.Next());
		}

		await That(results).AreAllUnique();
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextBytes_ShouldReturnSpecifiedValue(
		int seed, byte[] value)
	{
		List<byte[]> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length];
			random.NextBytes(buffer);
			results.Add(buffer);
		}

		await That(results).All().ComplyWith(v => v.IsEqualTo(value));
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextBytes_Span_ShouldReturnSpecifiedValue(
		int seed, byte[] value)
	{
		List<byte[]> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length];
			random.NextBytes(buffer.AsSpan());
			results.Add(buffer);
		}

		await That(results).All().ComplyWith(v => v.IsEqualTo(value));
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextBytes_Span_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<byte[]> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			byte[] buffer = new byte[10];
			random.NextBytes(buffer.AsSpan());
			results.Add(buffer);
		}

		await That(results).AreAllUnique();
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextBytes_Span_WithSmallerBuffer_ShouldReturnPartlyInitializedBytes(
			int seed, byte[] value)
	{
		List<byte[]> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length + 1];
			random.NextBytes(buffer.AsSpan());
			results.Add(buffer);
		}
		var expected = value.Concat(new[]
		{
			(byte)0,
		}).ToArray();

		await That(results).All().ComplyWith(v => v.IsEqualTo(expected));
	}
#endif

	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextBytes_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<byte[]> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			byte[] buffer = new byte[10];
			random.NextBytes(buffer);
			results.Add(buffer);
		}

		await That(results).AreAllUnique();
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextBytes_WithSmallerBuffer_ShouldReturnPartlyInitializedBytes(
			int seed, byte[] value)
	{
		List<byte[]> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(byteGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			byte[] buffer = new byte[value.Length + 1];
			random.NextBytes(buffer);
			results.Add(buffer);
		}
		var expected = value.Concat(new[]
		{
			(byte)0,
		}).ToArray();

		await That(results).All().ComplyWith(v => v.IsEqualTo(expected));
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextDouble_ShouldReturnSpecifiedValue(
		int seed, double value)
	{
		List<double> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(doubleGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextDouble());
		}

		await That(results).All().AreEqualTo(value);
	}

	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextDouble_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<double> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.NextDouble());
		}

		await That(results).AreAllUnique();
	}

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextInt64_ShouldReturnSpecifiedValue(int seed, long value)
	{
		List<long> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(longGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextInt64());
		}

		await That(results).All().AreEqualTo(value);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextInt64_WithMaxValue_ShouldReturnSpecifiedValue(
		int seed, long value)
	{
		long maxValue = value - 1;
		List<long> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(longGenerator: value);
		long expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextInt64(maxValue));
		}

		await That(results).All().AreEqualTo(expectedValue);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextInt64_WithMinAndMaxValue_Larger_ShouldReturnSpecifiedValue(
			int seed, long value)
	{
		long minValue = value - 10;
		long maxValue = value - 1;
		List<long> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(longGenerator: value);
		long expectedValue = maxValue - 1;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextInt64(minValue, maxValue));
		}

		await That(results).All().AreEqualTo(expectedValue);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextInt64_WithMinAndMaxValue_Smaller_ShouldReturnSpecifiedValue(
			int seed, long value)
	{
		long minValue = value + 1;
		long maxValue = minValue + 10;
		List<long> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(longGenerator: value);
		long expectedValue = minValue;

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextInt64(minValue, maxValue));
		}

		await That(results).All().AreEqualTo(expectedValue);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextInt64_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<long> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.NextInt64());
		}

		await That(results).AreAllUnique();
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextSingle_ShouldReturnSpecifiedValue(
		int seed, float value)
	{
		List<float> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate(singleGenerator: value);

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 100; i++)
		{
			results.Add(random.NextSingle());
		}

		await That(results).All().AreEqualTo(value);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Theory]
	[AutoData]
	public async Task GenerateRandom_NextSingle_WithoutGenerator_ShouldReturnRandomValues(
		int seed)
	{
		List<float> results = [];
		IRandomProvider randomProvider =
			RandomProvider.Generate();

		IRandom random = randomProvider.GetRandom(seed);
		for (int i = 0; i < 10; i++)
		{
			results.Add(random.NextSingle());
		}

		await That(results).AreAllUnique();
	}
#endif

	[Fact]
	public async Task GetRandom_DefaultValue_ShouldReturnSharedRandom()
	{
		RandomProviderMock randomProvider = new();
		IRandom random1 = randomProvider.GetRandom();
		IRandom random2 = randomProvider.GetRandom();

		int[] result1 = Enumerable.Range(0, 100)
			.Select(_ => random1.Next())
			.ToArray();
		int[] result2 = Enumerable.Range(0, 100)
			.Select(_ => random2.Next())
			.ToArray();

		await That(result1).IsNotEqualTo(result2).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task GetRandom_FixedSeed_ShouldReturnSeparateRandomInstances(int seed)
	{
		RandomProviderMock randomProvider = new();
		IRandom random1 = randomProvider.GetRandom(seed);
		IRandom random2 = randomProvider.GetRandom(seed);

		int[] result1 = Enumerable.Range(0, 100)
			.Select(_ => random1.Next())
			.ToArray();
		int[] result2 = Enumerable.Range(0, 100)
			.Select(_ => random2.Next())
			.ToArray();

		await That(result1).IsEqualTo(result2).InAnyOrder();
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task NextBytes_Span_WithoutByteGenerator_ShouldUseRealRandomValuesFromSeed(
		int seed)
	{
		Span<byte> result = new byte[100];
		Random random = new(seed);
		byte[] expectedBytes = new byte[result.Length];
		random.NextBytes(expectedBytes);
		IRandomProvider sut = RandomProvider.Generate(seed);

		sut.GetRandom().NextBytes(result);

		await That(result.ToArray()).IsEqualTo(expectedBytes);
	}
#endif

	[Theory]
	[AutoData]
	public async Task NextBytes_WithoutByteGenerator_ShouldUseRealRandomValuesFromSeed(
		int seed)
	{
		byte[] result = new byte[100];
		Random random = new(seed);
		byte[] expectedBytes = new byte[result.Length];
		random.NextBytes(expectedBytes);
		IRandomProvider sut = RandomProvider.Generate(seed);

		sut.GetRandom().NextBytes(result);

		await That(result).IsEqualTo(expectedBytes).InAnyOrder();
	}
}

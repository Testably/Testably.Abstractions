using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class RandomTests<TRandomSystem>
	: RandomSystemTestBase<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
#if FEATURE_RANDOM_ITEMS
	[Fact]
	public void GetItems_Array_EmptyChoices_ShouldThrowArgumentNullException()
	{
		int[] choices = Array.Empty<int>();

		Exception? exception = Record.Exception(() =>
		{
			RandomSystem.Random.Shared.GetItems(choices, 1);
		});

		exception.Should().BeException<ArgumentException>("Span may not be empty",
			hResult: -2147024809, paramName: nameof(choices));
	}

	[Fact]
	public void GetItems_Array_NullChoices_ShouldThrowArgumentNullException()
	{
		int[] choices = null!;

		Exception? exception = Record.Exception(() =>
		{
			RandomSystem.Random.Shared.GetItems(choices, -1);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

	[Fact]
	public void GetItems_Array_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		int[] choices = Enumerable.Range(0, 10).ToArray();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 100);

		result.Length.Should().Be(100);
		result.Should().OnlyContain(r => choices.Contains(r));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(-200)]
	public void GetItems_Array_NegativeLength_ShouldThrowArgumentOutOfRangeException(int length)
	{
		int[] choices = Enumerable.Range(0, 10).ToArray();

		Exception? exception = Record.Exception(() =>
		{
			RandomSystem.Random.Shared.GetItems(choices, length);
		});

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
			.Which.Message.Should().Be($"length ('{length}') must be a non-negative value. (Parameter 'length')\r\nActual value was {length}.");
	}

	[Fact]
	public void GetItems_Array_ShouldSelectRandomElements()
	{
		int[] choices = Enumerable.Range(0, 100).ToArray();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 10);

		result.Length.Should().Be(10);
		result.Should().OnlyContain(r => choices.Contains(r));
	}

	[Fact]
	public void GetItems_ReadOnlySpan_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		ReadOnlySpan<int> choices = Enumerable.Range(0, 10).ToArray().AsSpan();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 100);

		result.Length.Should().Be(100);
		result.Should().OnlyContain(r => r >= 0 && r < 10);
	}

	[Fact]
	public void GetItems_ReadOnlySpan_ShouldSelectRandomElements()
	{
		ReadOnlySpan<int> choices = Enumerable.Range(0, 100).ToArray().AsSpan();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 10);

		result.Length.Should().Be(10);
		result.Should().OnlyContain(r => r >= 0 && r < 100);
	}

	[Fact]
	public void GetItems_SpanDestination_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		int[] buffer = new int[100];
		Span<int> destination = new(buffer);
		ReadOnlySpan<int> choices = Enumerable.Range(0, 10).ToArray().AsSpan();

		RandomSystem.Random.Shared.GetItems(choices, destination);

		destination.Length.Should().Be(100);
		destination.ToArray().Should().OnlyContain(r => r >= 0 && r < 10);
	}

	[Fact]
	public void GetItems_SpanDestination_ShouldSelectRandomElements()
	{
		int[] buffer = new int[10];
		Span<int> destination = new(buffer);
		ReadOnlySpan<int> choices = Enumerable.Range(0, 100).ToArray().AsSpan();

		RandomSystem.Random.Shared.GetItems(choices, destination);

		destination.Length.Should().Be(10);
		destination.ToArray().Should().OnlyContain(r => r >= 0 && r < 100);
	}
#endif

	[SkippableFact]
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

	[SkippableFact]
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

	[SkippableFact]
	public void Next_ShouldBeThreadSafe()
	{
		ConcurrentBag<int> results = new();

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.Next());
		});

		results.Should().OnlyHaveUniqueItems();
	}

	[SkippableFact]
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
	[SkippableFact]
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

	[SkippableFact]
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
	[SkippableFact]
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

	[SkippableFact]
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

	[SkippableFact]
	public void NextInt64_ShouldBeThreadSafe()
	{
		ConcurrentBag<long> results = new();

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInt64());
		});

		results.Should().OnlyHaveUniqueItems();
	}

	[SkippableFact]
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

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public void Shuffle_Array_ShouldShuffleItemsInPlace()
	{
		int[] originalValues = Enumerable.Range(0, 100).ToArray();
		int[] values = originalValues.ToArray();

		RandomSystem.Random.Shared.Shuffle(values);

		values.Should().OnlyHaveUniqueItems();
		values.Should().NotContainInOrder(originalValues);
		values.OrderBy(x => x).Should().ContainInOrder(originalValues);
	}

	[Fact]
	public void Shuffle_Array_Null_ShouldThrowArgumentNullException()
	{
		int[] values = null!;

		Exception? exception = Record.Exception(() =>
		{
			RandomSystem.Random.Shared.Shuffle(values);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be(nameof(values));
	}

	[Fact]
	public void Shuffle_Span_ShouldShuffleItemsInPlace()
	{
		int[] originalValues = Enumerable.Range(0, 100).ToArray();
		int[] buffer = originalValues.ToArray();
		Span<int> values = new(buffer);

		RandomSystem.Random.Shared.Shuffle(values);

		int[] result = values.ToArray();
		result.Should().OnlyHaveUniqueItems();
		result.Should().NotContainInOrder(originalValues);
		result.OrderBy(x => x).Should().ContainInOrder(originalValues);
	}
#endif
}

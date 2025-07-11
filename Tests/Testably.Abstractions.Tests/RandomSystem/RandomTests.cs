using System.Collections.Concurrent;
#if FEATURE_RANDOM_ITEMS
using System.Linq;
#endif

namespace Testably.Abstractions.Tests.RandomSystem;

[RandomSystemTests]
public partial class RandomTests
{
#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_Array_EmptyChoices_ShouldThrowArgumentNullException()
	{
		int[] choices = Array.Empty<int>();

		void Act()
		{
			RandomSystem.Random.Shared.GetItems(choices, 1);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("Span may not be empty").AsPrefix().And
			.WithHResult(-2147024809).And
			.WithParamName(nameof(choices));
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_Array_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		int[] choices = Enumerable.Range(1, 10).ToArray();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 100);

		await That(result.Length).IsEqualTo(100);
		await That(result).All().Satisfy(r => choices.Contains(r));
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Theory]
	[InlineData(-1)]
	[InlineData(-200)]
	public async Task GetItems_Array_NegativeLength_ShouldThrowArgumentOutOfRangeException(int length)
	{
		int[] choices = Enumerable.Range(1, 10).ToArray();

		void Act()
		{
			RandomSystem.Random.Shared.GetItems(choices, length);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithMessage($"length ('{length}') must be a non-negative value. (Parameter 'length'){Environment.NewLine}Actual value was {length}.");
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_Array_NullChoices_ShouldThrowArgumentNullException()
	{
		int[] choices = null!;

		void Act()
		{
			RandomSystem.Random.Shared.GetItems(choices, -1);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>();
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_Array_ShouldSelectRandomElements()
	{
		int[] choices = Enumerable.Range(1, 100).ToArray();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 10);

		await That(result.Length).IsEqualTo(10);
		await That(result).All().Satisfy(r => choices.Contains(r));
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_ReadOnlySpan_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		ReadOnlySpan<int> choices = Enumerable.Range(1, 10).ToArray().AsSpan();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 100);

		await That(result.Length).IsEqualTo(100);
		await That(result).All().Satisfy(r => r >= 1 && r <= 10);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_ReadOnlySpan_ShouldSelectRandomElements()
	{
		ReadOnlySpan<int> choices = Enumerable.Range(1, 100).ToArray().AsSpan();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 10);

		await That(result.Length).IsEqualTo(10);
		await That(result).All().Satisfy(r => r >= 1 && r <= 100);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_SpanDestination_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		int[] buffer = new int[100];
		Span<int> destination = new(buffer);
		ReadOnlySpan<int> choices = Enumerable.Range(1, 10).ToArray().AsSpan();

		RandomSystem.Random.Shared.GetItems(choices, destination);

		var destinationArray = destination.ToArray();
		await That(destinationArray).All().Satisfy(r => r >= 1 && r <= 10);
		await That(destinationArray.Length).IsEqualTo(100);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task GetItems_SpanDestination_ShouldSelectRandomElements()
	{
		int[] buffer = new int[10];
		Span<int> destination = new(buffer);
		ReadOnlySpan<int> choices = Enumerable.Range(1, 100).ToArray().AsSpan();

		RandomSystem.Random.Shared.GetItems(choices, destination);

		var destinationArray = destination.ToArray();
		await That(destinationArray).All().Satisfy(r => r >= 1 && r <= 100);
		await That(destinationArray.Length).IsEqualTo(10);
	}
#endif
	[Fact]
	public async Task Next_MaxValue_ShouldOnlyReturnValidValues()
	{
		int maxValue = 10;
		ConcurrentBag<int> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.Next(maxValue));
		});

		await That(results).All().Satisfy(r => r < maxValue);
	}

	[Fact]
	public async Task Next_MinAndMaxValue_ShouldOnlyReturnValidValues()
	{
		int minValue = 10;
		int maxValue = 20;
		ConcurrentBag<int> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.Next(minValue, maxValue));
		});

		await That(results).All().Satisfy(r => r >= minValue && r < maxValue);
	}

	[Fact]
	public async Task Next_ShouldBeThreadSafe()
	{
		ConcurrentBag<int> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.Next());
		});

		await That(results).AreAllUnique();
	}

	[Fact]
	public async Task NextBytes_ShouldBeThreadSafe()
	{
		ConcurrentBag<byte[]> results = [];

		Parallel.For(0, 100, _ =>
		{
			byte[] bytes = new byte[100];
			RandomSystem.Random.Shared.NextBytes(bytes);
			results.Add(bytes);
		});

		await That(results).AreAllUnique();
	}

#if FEATURE_SPAN
	[Fact]
	public async Task NextBytes_Span_ShouldBeThreadSafe()
	{
		ConcurrentBag<byte[]> results = [];

		Parallel.For(0, 100, _ =>
		{
			Span<byte> bytes = new byte[100];
			RandomSystem.Random.Shared.NextBytes(bytes);
			results.Add(bytes.ToArray());
		});

		await That(results).AreAllUnique();
	}
#endif

	[Fact]
	public async Task NextDouble_ShouldBeThreadSafe()
	{
		ConcurrentBag<double> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextDouble());
		});

		await That(results).AreAllUnique();
	}

#if FEATURE_RANDOM_ADVANCED
	[Fact]
	public async Task NextInt64_MaxValue_ShouldOnlyReturnValidValues()
	{
		long maxValue = 10;
		ConcurrentBag<long> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInt64(maxValue));
		});

		await That(results).All().Satisfy(r => r < maxValue);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Fact]
	public async Task NextInt64_MinAndMaxValue_ShouldOnlyReturnValidValues()
	{
		long minValue = 10;
		long maxValue = 20;
		ConcurrentBag<long> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInt64(minValue, maxValue));
		});

		await That(results).All().Satisfy(r => r >= minValue && r < maxValue);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Fact]
	public async Task NextInt64_ShouldBeThreadSafe()
	{
		ConcurrentBag<long> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInt64());
		});

		await That(results).AreAllUnique();
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Fact]
	public async Task NextSingle_ShouldBeThreadSafe()
	{
		ConcurrentBag<float> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextSingle());
		});

		await That(results).AreAllUnique();
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task Shuffle_Array_Null_ShouldThrowArgumentNullException()
	{
		int[] values = null!;

		void Act()
		{
			RandomSystem.Random.Shared.Shuffle(values);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName(nameof(values));	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task Shuffle_Array_ShouldShuffleItemsInPlace()
	{
		int[] originalValues = Enumerable.Range(1, 100).ToArray();
		int[] values = originalValues.ToArray();

		RandomSystem.Random.Shared.Shuffle(values);

		await That(values).AreAllUnique();
		await That(values).DoesNotContain(originalValues);
		await That(values.Order()).Contains(originalValues);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Fact]
	public async Task Shuffle_Span_ShouldShuffleItemsInPlace()
	{
		int[] originalValues = Enumerable.Range(1, 100).ToArray();
		int[] buffer = originalValues.ToArray();
		Span<int> values = new(buffer);

		RandomSystem.Random.Shared.Shuffle(values);

		int[] result = values.ToArray();
		await That(result).AreAllUnique();
		await That(result).DoesNotContain(originalValues);
		await That(result.Order()).Contains(originalValues);
	}
#endif
}

using System.Collections.Concurrent;
#if FEATURE_RANDOM_ITEMS
using System.Linq;
#endif

namespace Testably.Abstractions.Tests.RandomSystem;

[RandomSystemTests]
public class RandomTests(RandomSystemTestData testData) : RandomSystemTestBase(testData)
{
#if FEATURE_RANDOM_STRINGS
	[Test]
	[Arguments(2)]
	[Arguments(100)]
	[Arguments(1000)]
	public async Task GetHexString_ShouldHaveExpectedLength(int length)
	{
		string result = RandomSystem.Random.Shared.GetHexString(length);

		await That(result).HasLength().EqualTo(length);
	}
#endif

#if FEATURE_RANDOM_STRINGS
	[Test]
	public async Task GetHexString_ShouldOnlyContainHexadecimalCharacters()
	{
		char[] hexadecimalCharacters = "0123456789ABCDEF".ToCharArray();

		string result = RandomSystem.Random.Shared.GetHexString(10000);

		await That(result.ToCharArray()).All().Satisfy(c => hexadecimalCharacters.Contains(c));
	}
#endif

#if FEATURE_RANDOM_STRINGS
	[Test]
	public async Task GetHexString_WithDestinationSpan_ShouldOnlyContainHexadecimalCharacters()
	{
		char[] buffer = new char[10000];
		Span<char> destination = new(buffer);
		char[] hexadecimalCharacters = "0123456789ABCDEF".ToCharArray();

		RandomSystem.Random.Shared.GetHexString(destination);

		char[] destinationArray = destination.ToArray();
		await That(destinationArray).All().Satisfy(c => hexadecimalCharacters.Contains(c));
	}
#endif

#if FEATURE_RANDOM_STRINGS
	[Test]
	public async Task
		GetHexString_WithDestinationSpan_WithLowercase_ShouldOnlyContainLowercaseHexadecimalCharacters()
	{
		char[] buffer = new char[10000];
		Span<char> destination = new(buffer);
		char[] hexadecimalCharacters = "0123456789abcdef".ToCharArray();

		RandomSystem.Random.Shared.GetHexString(destination, true);

		char[] destinationArray = destination.ToArray();
		await That(destinationArray).All().Satisfy(c => hexadecimalCharacters.Contains(c));
	}
#endif

#if FEATURE_RANDOM_STRINGS
	[Test]
	public async Task GetHexString_WithLowercase_ShouldOnlyContainLowercaseHexadecimalCharacters()
	{
		char[] hexadecimalCharacters = "0123456789abcdef".ToCharArray();

		string result = RandomSystem.Random.Shared.GetHexString(10000, true);

		await That(result.ToCharArray()).All().Satisfy(c => hexadecimalCharacters.Contains(c));
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
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
	[Test]
	public async Task GetItems_Array_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		int[] choices = Enumerable.Range(1, 10).ToArray();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 100);

		await That(result.Length).IsEqualTo(100);
		await That(result).All().Satisfy(r => choices.Contains(r));
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	[Arguments(-1)]
	[Arguments(-200)]
	public async Task GetItems_Array_NegativeLength_ShouldThrowArgumentOutOfRangeException(
		int length)
	{
		int[] choices = Enumerable.Range(1, 10).ToArray();

		void Act()
		{
			RandomSystem.Random.Shared.GetItems(choices, length);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithMessage(
				$"length ('{length}') must be a non-negative value. (Parameter 'length'){Environment.NewLine}Actual value was {length}.");
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
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
	[Test]
	public async Task GetItems_Array_ShouldSelectRandomElements()
	{
		int[] choices = Enumerable.Range(1, 100).ToArray();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 10);

		await That(result.Length).IsEqualTo(10);
		await That(result).All().Satisfy(r => choices.Contains(r));
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	public async Task GetItems_ReadOnlySpan_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		ReadOnlySpan<int> choices = Enumerable.Range(1, 10).ToArray().AsSpan();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 100);

		await That(result.Length).IsEqualTo(100);
		await That(result).All().Satisfy(r => r >= 1 && r <= 10);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	public async Task GetItems_ReadOnlySpan_ShouldSelectRandomElements()
	{
		ReadOnlySpan<int> choices = Enumerable.Range(1, 100).ToArray().AsSpan();

		int[] result = RandomSystem.Random.Shared.GetItems(choices, 10);

		await That(result.Length).IsEqualTo(10);
		await That(result).All().Satisfy(r => r >= 1 && r <= 100);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	public async Task
		GetItems_SpanDestination_LengthLargerThanChoices_ShouldIncludeDuplicateValues()
	{
		int[] buffer = new int[100];
		Span<int> destination = new(buffer);
		ReadOnlySpan<int> choices = Enumerable.Range(1, 10).ToArray().AsSpan();

		RandomSystem.Random.Shared.GetItems(choices, destination);

		int[] destinationArray = destination.ToArray();
		await That(destinationArray).All().Satisfy(r => r >= 1 && r <= 10);
		await That(destinationArray.Length).IsEqualTo(100);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	public async Task GetItems_SpanDestination_ShouldSelectRandomElements()
	{
		int[] buffer = new int[10];
		Span<int> destination = new(buffer);
		ReadOnlySpan<int> choices = Enumerable.Range(1, 100).ToArray().AsSpan();

		RandomSystem.Random.Shared.GetItems(choices, destination);

		int[] destinationArray = destination.ToArray();
		await That(destinationArray).All().Satisfy(r => r >= 1 && r <= 100);
		await That(destinationArray.Length).IsEqualTo(10);
	}
#endif

#if FEATURE_RANDOM_STRINGS
	[Test]
	[Arguments(2)]
	[Arguments(100)]
	[Arguments(1000)]
	public async Task GetString_ShouldHaveExpectedLength(int length)
	{
		ReadOnlySpan<char> choices = "abcde".ToCharArray();

		string result = RandomSystem.Random.Shared.GetString(choices, length);

		await That(result).HasLength().EqualTo(length);
	}
#endif

#if FEATURE_RANDOM_STRINGS
	[Test]
	[Arguments("foo")]
	public async Task GetString_ShouldOnlyContainProvidedCharacters(string value)
	{
		char[] chars = value.ToCharArray();
		ReadOnlySpan<char> choices = chars;

		string result = RandomSystem.Random.Shared.GetString(choices, 100);

		await That(result.ToCharArray()).All().Satisfy(c => chars.Contains(c));
	}
#endif

	[Test]
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

	[Test]
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

	[Test]
	public async Task Next_ShouldBeThreadSafe()
	{
		ConcurrentBag<int> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.Next());
		});

		await That(results).All().AreUnique();
	}

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextBinaryFloat_Double_ShouldBeThreadSafe()
	{
		ConcurrentBag<double> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextBinaryFloat<double>());
		});

		await That(results).All().AreUnique();
		await That(results).All().Satisfy(r => r is >= 0 and < 1);
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextBinaryFloat_Half_ShouldBeBetweenZeroAndOne()
	{
		ConcurrentBag<Half> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextBinaryFloat<Half>());
		});

		await That(results).All().Satisfy(r => r >= Half.Zero && r < Half.One);
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextBinaryFloat_Single_ShouldBeThreadSafe()
	{
		ConcurrentBag<float> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextBinaryFloat<float>());
		});

		await That(results).All().AreUnique();
		await That(results).All().Satisfy(r => r is >= 0 and < 1);
	}
#endif

	[Test]
	public async Task NextBytes_ShouldBeThreadSafe()
	{
		ConcurrentBag<byte[]> results = [];

		Parallel.For(0, 100, _ =>
		{
			byte[] bytes = new byte[100];
			RandomSystem.Random.Shared.NextBytes(bytes);
			results.Add(bytes);
		});

		await That(results).All().AreUnique();
	}

#if FEATURE_SPAN
	[Test]
	public async Task NextBytes_Span_ShouldBeThreadSafe()
	{
		ConcurrentBag<byte[]> results = [];

		Parallel.For(0, 100, _ =>
		{
			Span<byte> bytes = new byte[100];
			RandomSystem.Random.Shared.NextBytes(bytes);
			results.Add(bytes.ToArray());
		});

		await That(results).All().AreUnique();
	}
#endif

	[Test]
	public async Task NextDouble_ShouldBeThreadSafe()
	{
		ConcurrentBag<double> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextDouble());
		});

		await That(results).All().AreUnique();
	}

#if FEATURE_RANDOM_ADVANCED
	[Test]
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
	[Test]
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
	[Test]
	public async Task NextInt64_ShouldBeThreadSafe()
	{
		ConcurrentBag<long> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInt64());
		});

		await That(results).All().AreUnique();
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextInteger_Int_ShouldBeThreadSafe()
	{
		ConcurrentBag<int> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInteger<int>());
		});

		await That(results).All().AreUnique();
		await That(results).All().Satisfy(r => r >= 0);
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextInteger_Long_ShouldBeThreadSafe()
	{
		ConcurrentBag<long> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInteger<long>());
		});

		await That(results).All().AreUnique();
		await That(results).All().Satisfy(r => r >= 0);
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextInteger_SByte_ShouldNotReturnNegativeValues()
	{
		ConcurrentBag<sbyte> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextInteger<sbyte>());
		});

		await That(results).All().Satisfy(r => r >= 0);
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	[Arguments(-1)]
	[Arguments(-10)]
	public async Task NextInteger_WithMaxValue_Negative_ShouldThrowArgumentOutOfRangeException(
		int maxValue)
	{
		void Act()
		{
			RandomSystem.Random.Shared.NextInteger(maxValue);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName(nameof(maxValue));
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextInteger_WithMaxValue_ShouldOnlyReturnValidValues()
	{
		ConcurrentBag<int> intResults = [];
		ConcurrentBag<long> longResults = [];
		ConcurrentBag<ushort> ushortResults = [];

		Parallel.For(0, 100, _ =>
		{
			intResults.Add(RandomSystem.Random.Shared.NextInteger(10));
			longResults.Add(RandomSystem.Random.Shared.NextInteger(10L));
			ushortResults.Add(RandomSystem.Random.Shared.NextInteger((ushort)10));
		});

		await That(intResults).All().Satisfy(r => r is >= 0 and < 10);
		await That(longResults).All().Satisfy(r => r is >= 0 and < 10);
		await That(ushortResults).All().Satisfy(r => r < 10);
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextInteger_WithMinAndMaxValue_MinGreaterThanMax_ShouldThrowArgumentOutOfRangeException()
	{
		int minValue = 6;
		int maxValue = 5;

		void Act()
		{
			RandomSystem.Random.Shared.NextInteger(minValue, maxValue);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName(nameof(minValue));
	}
#endif

#if FEATURE_RANDOM_GENERIC
	[Test]
	public async Task NextInteger_WithMinAndMaxValue_ShouldOnlyReturnValidValues()
	{
		ConcurrentBag<int> intResults = [];
		ConcurrentBag<long> longResults = [];
		ConcurrentBag<short> shortResults = [];

		Parallel.For(0, 100, _ =>
		{
			intResults.Add(RandomSystem.Random.Shared.NextInteger(-10, 20));
			longResults.Add(RandomSystem.Random.Shared.NextInteger(-10L, 20L));
			shortResults.Add(RandomSystem.Random.Shared.NextInteger((short)-10, (short)20));
		});

		await That(intResults).All().Satisfy(r => r is >= -10 and < 20);
		await That(longResults).All().Satisfy(r => r is >= -10 and < 20);
		await That(shortResults).All().Satisfy(r => r is >= -10 and < 20);
	}
#endif

#if FEATURE_RANDOM_ADVANCED
	[Test]
	public async Task NextSingle_ShouldBeThreadSafe()
	{
		ConcurrentBag<float> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Random.Shared.NextSingle());
		});

		await That(results).All().AreUnique();
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	public async Task Shuffle_Array_Null_ShouldThrowArgumentNullException()
	{
		int[] values = null!;

		void Act()
		{
			RandomSystem.Random.Shared.Shuffle(values);
		}

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName(nameof(values));
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	public async Task Shuffle_Array_ShouldShuffleItemsInPlace()
	{
		int[] originalValues = Enumerable.Range(1, 100).ToArray();
		int[] values = originalValues.ToArray();

		RandomSystem.Random.Shared.Shuffle(values);

		await That(values).All().AreUnique();
		await That(values).DoesNotContain(originalValues);
		await That(values.Order()).Contains(originalValues);
	}
#endif

#if FEATURE_RANDOM_ITEMS
	[Test]
	public async Task Shuffle_Span_ShouldShuffleItemsInPlace()
	{
		int[] originalValues = Enumerable.Range(1, 100).ToArray();
		int[] buffer = originalValues.ToArray();
		Span<int> values = new(buffer);

		RandomSystem.Random.Shared.Shuffle(values);

		int[] result = values.ToArray();
		await That(result).All().AreUnique();
		await That(result).DoesNotContain(originalValues);
		await That(result.Order()).Contains(originalValues);
	}
#endif
}

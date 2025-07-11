using System.Globalization;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class ParameterDescriptionTests
{
	[Theory]
	[AutoData]
	public async Task FromOutParameter_ShouldSetIsOutParameterToTrue(int value)
	{
		ParameterDescription sut = ParameterDescription.FromOutParameter(value);

		await That(sut.IsOutParameter).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task FromParameter_ShouldSetIsOutParameterToFalse(int value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		await That(sut.IsOutParameter).IsFalse();
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task FromParameter_WithReadOnlySpan_ShouldSetIsOutParameterToFalse(string buffer)
	{
		ReadOnlySpan<char> value = buffer.AsSpan();

		ParameterDescription sut = ParameterDescription.FromParameter(value);

		await That(sut.IsOutParameter).IsFalse();
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task FromParameter_WithSpan_ShouldSetIsOutParameterToFalse(int[] buffer)
	{
		Span<int> value = buffer.AsSpan();

		ParameterDescription sut = ParameterDescription.FromParameter(value);

		await That(sut.IsOutParameter).IsFalse();
	}
#endif

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public async Task Is_WithComparer_ShouldUseComparerResult(bool comparerResult, string value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		bool result = sut.Is<string>(_ => comparerResult);

		await That(result).IsEqualTo(comparerResult);
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public async Task Is_WithComparer_WithIncompatibleType_ShouldReturnFalse(bool comparerResult,
		string value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		bool result = sut.Is<int>(_ => comparerResult);

		await That(result).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Is_WithFromOutParameter_ShouldCheckForMatchingValue(int value)
	{
		ParameterDescription sut = ParameterDescription.FromOutParameter(value);

		await That(sut.Is(value)).IsTrue();
		await That(sut.Is(value + 1)).IsFalse();
		await That(sut.Is("foo")).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Is_WithFromParameter_ShouldCheckForMatchingValue(string value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		await That(sut.Is(value)).IsTrue();
		await That(sut.Is($"other_{value}")).IsFalse();
		await That(sut.Is(42)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task ToString_ShouldReturnValue(int value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		string? result = sut.ToString();

		await That(result).IsEqualTo(value.ToString(CultureInfo.InvariantCulture));
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task ToString_WithReadOnlySpan_ShouldSetIsOutParameterToFalse(string buffer)
	{
		ReadOnlySpan<char> value = buffer.AsSpan();
		ParameterDescription sut = ParameterDescription.FromParameter(value);
		string expectedString = $"[{string.Join(',', buffer.ToCharArray())}]";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedString);
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public async Task ToString_WithSpan_ShouldSetIsOutParameterToFalse(int[] buffer)
	{
		Span<int> value = buffer.AsSpan();
		ParameterDescription sut = ParameterDescription.FromParameter(value);
		string expectedString = $"[{string.Join(',', buffer)}]";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedString);
	}
#endif

	[Theory]
	[AutoData]
	public async Task ToString_WithStringValue_ShouldReturnValueEnclosedInQuotationMarks(
		string value)
	{
		ParameterDescription sut = ParameterDescription.FromOutParameter(value);

		string? result = sut.ToString();

		await That(result).IsEqualTo($"\"{value}\"");
	}
}

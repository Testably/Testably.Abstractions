using System.Globalization;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed class ParameterDescriptionTests
{
	[Theory]
	[AutoData]
	public void FromOutParameter_ShouldSetIsOutParameterToTrue(int value)
	{
		ParameterDescription sut = ParameterDescription.FromOutParameter(value);

		sut.IsOutParameter.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void FromParameter_ShouldSetIsOutParameterToFalse(int value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		sut.IsOutParameter.Should().BeFalse();
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void FromParameter_WithReadOnlySpan_ShouldSetIsOutParameterToFalse(string buffer)
	{
		ReadOnlySpan<char> value = buffer.AsSpan();

		ParameterDescription sut = ParameterDescription.FromParameter(value);

		sut.IsOutParameter.Should().BeFalse();
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void FromParameter_WithSpan_ShouldSetIsOutParameterToFalse(int[] buffer)
	{
		Span<int> value = buffer.AsSpan();

		ParameterDescription sut = ParameterDescription.FromParameter(value);

		sut.IsOutParameter.Should().BeFalse();
	}
#endif

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public void Is_WithComparer_ShouldUseComparerResult(bool comparerResult, string value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		bool result = sut.Is<string>(_ => comparerResult);

		result.Should().Be(comparerResult);
	}

	[Theory]
	[InlineAutoData(true)]
	[InlineAutoData(false)]
	public void Is_WithComparer_WithIncompatibleType_ShouldReturnFalse(bool comparerResult,
		string value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		bool result = sut.Is<int>(_ => comparerResult);

		result.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Is_WithFromOutParameter_ShouldCheckForMatchingValue(int value)
	{
		ParameterDescription sut = ParameterDescription.FromOutParameter(value);

		sut.Is(value).Should().BeTrue();
		sut.Is(value + 1).Should().BeFalse();
		sut.Is("foo").Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Is_WithFromParameter_ShouldCheckForMatchingValue(string value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		sut.Is(value).Should().BeTrue();
		sut.Is($"other_{value}").Should().BeFalse();
		sut.Is(42).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void ToString_ShouldReturnValue(int value)
	{
		ParameterDescription sut = ParameterDescription.FromParameter(value);

		string? result = sut.ToString();

		result.Should().Be(value.ToString(CultureInfo.InvariantCulture));
	}

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void ToString_WithReadOnlySpan_ShouldSetIsOutParameterToFalse(string buffer)
	{
		ReadOnlySpan<char> value = buffer.AsSpan();
		ParameterDescription sut = ParameterDescription.FromParameter(value);
		string expectedString = $"[{string.Join(",", buffer.ToCharArray())}]";

		string? result = sut.ToString();

		result.Should().Be(expectedString);
	}
#endif

#if FEATURE_SPAN
	[Theory]
	[AutoData]
	public void ToString_WithSpan_ShouldSetIsOutParameterToFalse(int[] buffer)
	{
		Span<int> value = buffer.AsSpan();
		ParameterDescription sut = ParameterDescription.FromParameter(value);
		string expectedString = $"[{string.Join(",", buffer)}]";

		string? result = sut.ToString();

		result.Should().Be(expectedString);
	}
#endif

	[Theory]
	[AutoData]
	public void ToString_WithStringValue_ShouldReturnValueEnclosedInQuotationMarks(string value)
	{
		ParameterDescription sut = ParameterDescription.FromOutParameter(value);

		string? result = sut.ToString();

		result.Should().Be($"\"{value}\"");
	}
}

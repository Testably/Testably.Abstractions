using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GuidTests<TRandomSystem>
	: RandomSystemTestBase<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
	[Fact]
	public void Empty_ShouldReturnEmptyGuid()
	{
		RandomSystem.Guid.Empty.Should().Be(System.Guid.Empty);
	}

	[Fact]
	public void NewGuid_ShouldBeThreadSafeAndReturnUniqueItems()
	{
		ConcurrentBag<System.Guid> results = new();

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Guid.NewGuid());
		});

		results.Should().OnlyHaveUniqueItems();
	}

#if FEATURE_GUID_PARSE
	[Theory]
	[AutoData]
	public void Parse_String_ShouldReturnCorrectGuid(System.Guid guid)
	{
		string serializedGuid = guid.ToString();

		System.Guid result = RandomSystem.Guid.Parse(serializedGuid);

		result.Should().Be(guid);
	}

	[Theory]
	[AutoData]
	public void Parse_SpanArray_ShouldReturnCorrectGuid(System.Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

		System.Guid result = RandomSystem.Guid.Parse(serializedGuid);

		result.Should().Be(guid);
	}

	[Theory]
	[AutoData]
	public void TryParse_String_ShouldReturnTrue(System.Guid guid)
	{
		string serializedGuid = guid.ToString();

		bool result = RandomSystem.Guid.TryParse(serializedGuid, out System.Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	[Theory]
	[AutoData]
	public void TryParse_SpanArray_ShouldReturnTrue(System.Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

		bool result = RandomSystem.Guid.TryParse(serializedGuid, out System.Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	[Theory]
	[MemberAutoData(nameof(GuidFormats))]
	public void ParseExact_String_ShouldReturnCorrectGuid(string format, System.Guid guid)
	{
		string serializedGuid = guid.ToString(format);

		System.Guid result = RandomSystem.Guid.ParseExact(serializedGuid, format);

		result.Should().Be(guid);
	}

	[Theory]
	[MemberAutoData(nameof(GuidFormats))]
	public void ParseExact_SpanArray_ShouldReturnCorrectGuid(
		string format, System.Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString(format).AsSpan();

		System.Guid result = RandomSystem.Guid.ParseExact(serializedGuid, format);

		result.Should().Be(guid);
	}

	[Theory]
	[MemberAutoData(nameof(GuidFormats))]
	public void TryParseExact_String_ShouldReturnTrue(string format, System.Guid guid)
	{
		string serializedGuid = guid.ToString(format);

		bool result =
			RandomSystem.Guid.TryParseExact(serializedGuid, format,
				out System.Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	[Theory]
	[MemberAutoData(nameof(GuidFormats))]
	public void TryParseExact_SpanArray_ShouldReturnTrue(string format, System.Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString(format).AsSpan();

		bool result =
			RandomSystem.Guid.TryParseExact(serializedGuid, format,
				out System.Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	public static IEnumerable<object[]> GuidFormats()
	{
		yield return new object[] { "N" };
		yield return new object[] { "D" };
		yield return new object[] { "B" };
		yield return new object[] { "P" };
		yield return new object[] { "X" };
	}
#endif
}
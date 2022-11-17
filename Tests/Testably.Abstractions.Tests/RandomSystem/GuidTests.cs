using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
#if FEATURE_GUID_PARSE
using System.Collections.Generic;
#endif

namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GuidTests<TRandomSystem>
	: RandomSystemTestBase<TRandomSystem>
	where TRandomSystem : IRandomSystem
{
	[SkippableFact]
	public void Empty_ShouldReturnEmptyGuid()
	{
		RandomSystem.Guid.Empty.Should().Be(Guid.Empty);
	}

	[SkippableFact]
	public void NewGuid_ShouldBeThreadSafeAndReturnUniqueItems()
	{
		ConcurrentBag<Guid> results = new();

		Parallel.For(0, 100, _ =>
		{
			results.Add(RandomSystem.Guid.NewGuid());
		});

		results.Should().OnlyHaveUniqueItems();
	}

#if FEATURE_GUID_PARSE
	[SkippableTheory]
	[AutoData]
	public void Parse_String_ShouldReturnCorrectGuid(Guid guid)
	{
		string serializedGuid = guid.ToString();

		Guid result = RandomSystem.Guid.Parse(serializedGuid);

		result.Should().Be(guid);
	}

	[SkippableTheory]
	[AutoData]
	public void Parse_SpanArray_ShouldReturnCorrectGuid(Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

		Guid result = RandomSystem.Guid.Parse(serializedGuid);

		result.Should().Be(guid);
	}
#endif

#if FEATURE_GUID_FORMATPROVIDER
	[SkippableTheory]
	[AutoData]
	public void Parse_WithFormatProvider_String_ShouldReturnCorrectGuid(Guid guid)
	{
		string serializedGuid = guid.ToString();

		Guid result = RandomSystem.Guid.Parse(serializedGuid, CultureInfo.InvariantCulture);

		result.Should().Be(guid);
	}

	[SkippableTheory]
	[AutoData]
	public void Parse_WithFormatProvider_SpanArray_ShouldReturnCorrectGuid(Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

		Guid result = RandomSystem.Guid.Parse(serializedGuid, CultureInfo.InvariantCulture);

		result.Should().Be(guid);
	}
#endif

#if FEATURE_GUID_PARSE
	[SkippableTheory]
	[MemberAutoData(nameof(GuidFormats))]
	public void ParseExact_String_ShouldReturnCorrectGuid(string format, Guid guid)
	{
		string serializedGuid = guid.ToString(format);

		Guid result = RandomSystem.Guid.ParseExact(serializedGuid, format);

		result.Should().Be(guid);
	}

	[SkippableTheory]
	[MemberAutoData(nameof(GuidFormats))]
	public void ParseExact_SpanArray_ShouldReturnCorrectGuid(
		string format, Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString(format).AsSpan();

		Guid result = RandomSystem.Guid.ParseExact(serializedGuid, format);

		result.Should().Be(guid);
	}
#endif

#if FEATURE_GUID_PARSE
	[SkippableTheory]
	[AutoData]
	public void TryParse_String_ShouldReturnTrue(Guid guid)
	{
		string serializedGuid = guid.ToString();

		bool result = RandomSystem.Guid.TryParse(serializedGuid, out Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	[SkippableTheory]
	[AutoData]
	public void TryParse_SpanArray_ShouldReturnTrue(Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

		bool result = RandomSystem.Guid.TryParse(serializedGuid, out Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}
#endif

#if FEATURE_GUID_FORMATPROVIDER
	[SkippableTheory]
	[AutoData]
	public void TryParse_WithFormatProvider_String_ShouldReturnTrue(Guid guid)
	{
		string serializedGuid = guid.ToString();

		bool result = RandomSystem.Guid.TryParse(serializedGuid, CultureInfo.InvariantCulture,
			out Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	[SkippableTheory]
	[AutoData]
	public void TryParse_WithFormatProvider_SpanArray_ShouldReturnTrue(Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

		bool result = RandomSystem.Guid.TryParse(serializedGuid, CultureInfo.InvariantCulture,
			out Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}
#endif

#if FEATURE_GUID_PARSE
	[SkippableTheory]
	[MemberAutoData(nameof(GuidFormats))]
	public void TryParseExact_String_ShouldReturnTrue(string format, Guid guid)
	{
		string serializedGuid = guid.ToString(format);

		bool result =
			RandomSystem.Guid.TryParseExact(serializedGuid, format,
				out Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	[SkippableTheory]
	[MemberAutoData(nameof(GuidFormats))]
	public void TryParseExact_SpanArray_ShouldReturnTrue(string format, Guid guid)
	{
		ReadOnlySpan<char> serializedGuid = guid.ToString(format).AsSpan();

		bool result =
			RandomSystem.Guid.TryParseExact(serializedGuid, format,
				out Guid value);

		result.Should().BeTrue();
		value.Should().Be(guid);
	}

	public static IEnumerable<object[]> GuidFormats()
	{
		yield return new object[]
		{
			"N"
		};
		yield return new object[]
		{
			"D"
		};
		yield return new object[]
		{
			"B"
		};
		yield return new object[]
		{
			"P"
		};
		yield return new object[]
		{
			"X"
		};
	}
#endif
}

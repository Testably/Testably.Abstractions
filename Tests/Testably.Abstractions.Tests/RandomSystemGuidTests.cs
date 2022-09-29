using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract class RandomSystemGuidTests<TRandomSystem>
    where TRandomSystem : IRandomSystem
{
    #region Test Setup

    public TRandomSystem RandomSystem { get; }

    protected RandomSystemGuidTests(TRandomSystem randomSystem)
    {
        RandomSystem = randomSystem;
    }

    #endregion

    [Fact]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.Empty))]
    public void Empty_ShouldReturnEmptyGuid()
    {
        RandomSystem.Guid.Empty.Should().Be(Guid.Empty);
    }

    [Fact]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.NewGuid))]
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
    [Theory]
    [AutoData]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.Parse))]
    public void Parse_String_ShouldReturnCorrectGuid(Guid guid)
    {
        string serializedGuid = guid.ToString();

        Guid result = RandomSystem.Guid.Parse(serializedGuid);

        result.Should().Be(guid);
    }

    [Theory]
    [AutoData]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.Parse))]
    public void Parse_SpanArray_ShouldReturnCorrectGuid(Guid guid)
    {
        ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

        Guid result = RandomSystem.Guid.Parse(serializedGuid);

        result.Should().Be(guid);
    }

    [Theory]
    [AutoData]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.TryParse))]
    public void TryParse_String_ShouldReturnTrue(Guid guid)
    {
        string serializedGuid = guid.ToString();

        bool result = RandomSystem.Guid.TryParse(serializedGuid, out Guid value);

        result.Should().BeTrue();
        value.Should().Be(guid);
    }

    [Theory]
    [AutoData]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.TryParse))]
    public void TryParse_SpanArray_ShouldReturnTrue(Guid guid)
    {
        ReadOnlySpan<char> serializedGuid = guid.ToString().AsSpan();

        bool result = RandomSystem.Guid.TryParse(serializedGuid, out Guid value);

        result.Should().BeTrue();
        value.Should().Be(guid);
    }

    [Theory]
    [MemberAutoData(nameof(GuidFormats))]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.ParseExact))]
    public void ParseExact_String_ShouldReturnCorrectGuid(string format, Guid guid)
    {
        string serializedGuid = guid.ToString(format);

        Guid result = RandomSystem.Guid.ParseExact(serializedGuid, format);

        result.Should().Be(guid);
    }

    [Theory]
    [MemberAutoData(nameof(GuidFormats))]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.ParseExact))]
    public void ParseExact_SpanArray_ShouldReturnCorrectGuid(string format, Guid guid)
    {
        ReadOnlySpan<char> serializedGuid = guid.ToString(format).AsSpan();

        Guid result = RandomSystem.Guid.ParseExact(serializedGuid, format);

        result.Should().Be(guid);
    }

    [Theory]
    [MemberAutoData(nameof(GuidFormats))]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.TryParseExact))]
    public void TryParseExact_String_ShouldReturnTrue(string format, Guid guid)
    {
        string serializedGuid = guid.ToString(format);

        bool result =
            RandomSystem.Guid.TryParseExact(serializedGuid, format, out Guid value);

        result.Should().BeTrue();
        value.Should().Be(guid);
    }

    [Theory]
    [MemberAutoData(nameof(GuidFormats))]
    [RandomSystemTests.Guid(nameof(IRandomSystem.IGuid.TryParseExact))]
    public void TryParseExact_SpanArray_ShouldReturnTrue(string format, Guid guid)
    {
        ReadOnlySpan<char> serializedGuid = guid.ToString(format).AsSpan();

        bool result =
            RandomSystem.Guid.TryParseExact(serializedGuid, format, out Guid value);

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
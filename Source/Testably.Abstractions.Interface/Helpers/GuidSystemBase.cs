using System;
using Testably.Abstractions.RandomSystem;
#if FEATURE_GUID_PARSE
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.Helpers;

/// <summary>
///     Default implementation for <see cref="System.Guid" /> abstractions.
///     <para />
///     Implements <seealso cref="IGuid" />
/// </summary>
public abstract class GuidSystemBase : IGuid
{
	/// <summary>
	///     Initializes a new instance of <see cref="GuidSystemBase" /> for the given <paramref name="randomSystem" />.
	/// </summary>
	protected GuidSystemBase(IRandomSystem randomSystem)
	{
		RandomSystem = randomSystem;
	}

	#region IGuid Members

	/// <inheritdoc cref="IGuid.Empty" />
	public Guid Empty => Guid.Empty;

	/// <inheritdoc cref="IRandomSystemEntity.RandomSystem" />
	public IRandomSystem RandomSystem { get; }

#if FEATURE_GUID_V7
	/// <inheritdoc cref="IGuid.CreateVersion7()" />
	public abstract Guid CreateVersion7();
#endif

#if FEATURE_GUID_V7
	/// <inheritdoc cref="IGuid.CreateVersion7(DateTimeOffset)" />
	public abstract Guid CreateVersion7(DateTimeOffset timestamp);
#endif

	/// <inheritdoc cref="IGuid.NewGuid()" />
	public abstract Guid NewGuid();

#if FEATURE_GUID_PARSE
	#pragma warning disable MA0011
	/// <inheritdoc cref="IGuid.Parse(string)" />
	public Guid Parse(string input)
		=> Guid.Parse(input);
	#pragma warning restore MA0011
#endif

#if FEATURE_GUID_PARSE
	#pragma warning disable MA0011
	/// <inheritdoc cref="IGuid.Parse(ReadOnlySpan{char})" />
	public Guid Parse(ReadOnlySpan<char> input)
		=> Guid.Parse(input);
	#pragma warning restore MA0011
#endif

#if FEATURE_GUID_PARSE_UTF8
	/// <inheritdoc cref="IGuid.Parse(ReadOnlySpan{byte})" />
	public Guid Parse(ReadOnlySpan<byte> utf8Text)
		=> Guid.Parse(utf8Text);
#endif

#if FEATURE_GUID_FORMATPROVIDER
	/// <inheritdoc cref="IGuid.Parse(string, IFormatProvider?)" />
	public Guid Parse(string s, IFormatProvider? provider)
		=> Guid.Parse(s, provider);
#endif

#if FEATURE_GUID_FORMATPROVIDER
	/// <inheritdoc cref="IGuid.Parse(ReadOnlySpan{char}, IFormatProvider?)" />
	public Guid Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
		=> Guid.Parse(s, provider);
#endif

#if FEATURE_GUID_PARSE_UTF8
	/// <inheritdoc cref="IGuid.Parse(ReadOnlySpan{byte}, IFormatProvider)" />
	public Guid Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider provider)
		=> Guid.Parse(utf8Text, provider);
#endif

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="IGuid.ParseExact(string, string)" />
	public Guid ParseExact(string input, string format)
		=> Guid.ParseExact(input, format);
#endif

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="IGuid.ParseExact(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public Guid ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format)
		=> Guid.ParseExact(input, format);
#endif

#if FEATURE_GUID_PARSE
	#pragma warning disable MA0011
	/// <inheritdoc cref="IGuid.TryParse(string, out Guid)" />
	public bool TryParse(string? input, out Guid result)
		=> Guid.TryParse(input, out result);
	#pragma warning restore MA0011
#endif

#if FEATURE_GUID_PARSE
	#pragma warning disable MA0011
	/// <inheritdoc cref="IGuid.TryParse(ReadOnlySpan{char}, out Guid)" />
	public bool TryParse(ReadOnlySpan<char> input, out Guid result)
		=> Guid.TryParse(input, out result);
	#pragma warning restore MA0011
#endif

#if FEATURE_GUID_FORMATPROVIDER
	/// <inheritdoc cref="IGuid.TryParse(string, IFormatProvider?, out Guid)" />
	public bool TryParse(string? s, IFormatProvider? provider, out Guid result)
		=> Guid.TryParse(s, provider, out result);
#endif

#if FEATURE_GUID_FORMATPROVIDER
	/// <inheritdoc cref="IGuid.TryParse(ReadOnlySpan{char}, IFormatProvider?, out Guid)" />
	public bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Guid result)
		=> Guid.TryParse(s, provider, out result);
#endif

#if FEATURE_GUID_PARSE_UTF8
	/// <inheritdoc cref="IGuid.TryParse(ReadOnlySpan{byte}, out Guid)" />
	public bool TryParse(ReadOnlySpan<byte> utf8Text, out Guid result)
		=> Guid.TryParse(utf8Text, out result);
#endif

#if FEATURE_GUID_PARSE_UTF8
	/// <inheritdoc cref="IGuid.TryParse(ReadOnlySpan{byte}, IFormatProvider, out Guid)" />
	public bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider provider, out Guid result)
		=> Guid.TryParse(utf8Text, provider, out result);
#endif

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="IGuid.TryParseExact(string?, string?, out Guid)" />
	public bool TryParseExact([NotNullWhen(true)] string? input,
		[NotNullWhen(true)] string? format,
		out Guid result)
		=> Guid.TryParseExact(input, format, out result);
#endif

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="IGuid.TryParseExact(ReadOnlySpan{char}, ReadOnlySpan{char}, out Guid)" />
	public bool TryParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format,
		out Guid result)
		=> Guid.TryParseExact(input, format, out result);
#endif

	#endregion
}

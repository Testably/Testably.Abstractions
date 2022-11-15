using System;
#if FEATURE_GUID_PARSE
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.RandomSystem;

/// <summary>
///     Abstractions for <see cref="Guid" />.
/// </summary>
public interface IGuid : IRandomSystemEntity
{
	/// <inheritdoc cref="Guid.Empty" />
	Guid Empty { get; }

	/// <inheritdoc cref="Guid.NewGuid()" />
	Guid NewGuid();

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="Guid.Parse(string)" />
	Guid Parse(string input);

	/// <inheritdoc cref="Guid.Parse(ReadOnlySpan{char})" />
	Guid Parse(ReadOnlySpan<char> input);
#endif

#if FEATURE_GUID_FORMATPROVIDER
	/// <inheritdoc cref="Guid.Parse(string, IFormatProvider?)" />
	Guid Parse(string s, IFormatProvider? provider);

	/// <inheritdoc cref="Guid.Parse(ReadOnlySpan{char}, IFormatProvider?)" />
	Guid Parse(ReadOnlySpan<char> s, IFormatProvider? provider);
#endif

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="Guid.TryParse(string?, out Guid)" />
	bool TryParse([NotNullWhen(true)] string? input, out Guid result);

	/// <inheritdoc cref="Guid.TryParse(ReadOnlySpan{char}, out Guid)" />
	bool TryParse(ReadOnlySpan<char> input, out Guid result);
#endif

#if FEATURE_GUID_FORMATPROVIDER
	/// <inheritdoc cref="Guid.TryParse(string?, IFormatProvider?, out Guid)" />
	bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Guid result);

	/// <inheritdoc cref="Guid.TryParse(ReadOnlySpan{char}, IFormatProvider?, out Guid)" />
	bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Guid result);
#endif

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="Guid.ParseExact(string, string)" />
	Guid ParseExact(string input, string format);

	/// <inheritdoc cref="Guid.ParseExact(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	Guid ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format);

	/// <inheritdoc cref="Guid.TryParseExact(string?, string?, out Guid)" />
	bool TryParseExact([NotNullWhen(true)] string? input,
		[NotNullWhen(true)] string? format,
		out Guid result);

	/// <inheritdoc cref="Guid.TryParseExact(ReadOnlySpan{char}, ReadOnlySpan{char}, out Guid)" />
	bool TryParseExact(ReadOnlySpan<char> input,
		ReadOnlySpan<char> format,
		out Guid result);
#endif
}

using System;
#if FEATURE_GUID_PARSE
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions;

/// <summary>
///     Abstractions for <see cref="System.Guid" />.
/// </summary>
public interface IGuid : IRandomSystemExtensionPoint
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

	/// <inheritdoc cref="Guid.TryParse(string?, out Guid)" />
	bool TryParse([NotNullWhen(true)] string? input, out Guid result);

	/// <inheritdoc cref="Guid.TryParse(ReadOnlySpan{char}, out Guid)" />
	bool TryParse(ReadOnlySpan<char> input, out Guid result);

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
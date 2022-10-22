using System;
#if FEATURE_GUID_PARSE
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.Helpers;

/// <summary>
///     Default implementation for <see cref="System.Guid" /> abstractions.
///     <para />
///     Implements <seealso cref="IRandomSystem.IGuid" />
/// </summary>
public abstract class GuidSystemBase : IRandomSystem.IGuid
{
	/// <summary>
	///     Initializes a new instance of <see cref="GuidSystemBase" /> for the given <paramref name="randomSystem" />.
	/// </summary>
	protected GuidSystemBase(IRandomSystem randomSystem)
	{
		RandomSystem = randomSystem;
	}

	#region IGuid Members

	/// <inheritdoc cref="IRandomSystem.IGuid.Empty" />
	public Guid Empty => Guid.Empty;

	/// <inheritdoc cref="IRandomSystem.IRandomSystemExtensionPoint.RandomSystem" />
	public IRandomSystem RandomSystem { get; }

	/// <inheritdoc cref="IRandomSystem.IGuid.NewGuid()" />
	public abstract Guid NewGuid();

	#endregion

#if FEATURE_GUID_PARSE
	/// <inheritdoc cref="IRandomSystem.IGuid.Parse(string)" />
	public Guid Parse(string input)
		=> Guid.Parse(input);

	/// <inheritdoc cref="IRandomSystem.IGuid.Parse(ReadOnlySpan{char})" />
	public Guid Parse(ReadOnlySpan<char> input)
		=> Guid.Parse(input);

	/// <inheritdoc cref="IRandomSystem.IGuid.TryParse(string, out Guid)" />
	public bool TryParse(string? input, out Guid result)
		=> Guid.TryParse(input, out result);

	/// <inheritdoc cref="IRandomSystem.IGuid.TryParse(ReadOnlySpan{char}, out Guid)" />
	public bool TryParse(ReadOnlySpan<char> input, out Guid result)
		=> Guid.TryParse(input, out result);

	/// <inheritdoc cref="IRandomSystem.IGuid.ParseExact(string, string)" />
	public Guid ParseExact(string input, string format)
		=> Guid.ParseExact(input, format);

	/// <inheritdoc cref="IRandomSystem.IGuid.ParseExact(ReadOnlySpan{char}, ReadOnlySpan{char})" />
	public Guid ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format)
		=> Guid.ParseExact(input, format);

	/// <inheritdoc cref="IRandomSystem.IGuid.TryParseExact(string?, string?, out Guid)" />
	public bool TryParseExact([NotNullWhen(true)] string? input,
	                          [NotNullWhen(true)] string? format,
	                          out Guid result)
		=> Guid.TryParseExact(input, format, out result);

	/// <inheritdoc cref="IRandomSystem.IGuid.TryParseExact(ReadOnlySpan{char}, ReadOnlySpan{char}, out Guid)" />
	public bool TryParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format,
	                          out Guid result)
		=> Guid.TryParseExact(input, format, out result);
#endif
}
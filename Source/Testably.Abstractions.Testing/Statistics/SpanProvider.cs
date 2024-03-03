#if FEATURE_SPAN
using System;

namespace Testably.Abstractions.Testing.Statistics;

public class SpanProvider<T>
{
	public T[] Values { get; }
	public bool IsReadOnly { get; }

	public SpanProvider(Span<T> values)
	{
		Values = values.ToArray();
		IsReadOnly = false;
	}

	public SpanProvider(ReadOnlySpan<T> values)
	{
		Values = values.ToArray();
		IsReadOnly = true;
	}

	public static implicit operator Span<T>(SpanProvider<T> provider)
	{
		if (provider.IsReadOnly)
		{
			throw new InvalidCastException("The Span provider is read-only.");
		}

		return provider.Values.AsSpan();
	}

	public static implicit operator ReadOnlySpan<T>(SpanProvider<T> provider)
	{
		if (!provider.IsReadOnly)
		{
			throw new InvalidCastException("The Span provider is writable.");
		}

		return provider.Values.AsSpan();
	}
}
#endif

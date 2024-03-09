using System;
using System.Linq;

namespace Testably.Abstractions.Testing.Statistics;

public abstract class ParameterDescription
{
	protected ParameterDescription(bool isOutParameter)
	{
		IsOutParameter = isOutParameter;
	}

	public bool IsOutParameter { get; }

	public bool Is<T>(T value)
		=> this is GenericParameterDescription<T> d &&
		   IsEqual(value, d.Value);

	public bool Is<T>(T[] value)
	{
		return this is GenericParameterDescription<T[]> d &&
		       value.SequenceEqual(d.Value);
	}

	private static bool IsEqual<T>(T value1, T value2)
	{
		if (value1 == null)
		{
			return value2 == null;
		}
		return value1.Equals(value2);
	}
#if FEATURE_SPAN
	public bool Is<T>(Span<T> value)
		=> this is SpanParameterDescription<T> { IsReadOnly: false } d &&
		   d.Value.SequenceEqual(value.ToArray());

	public bool Is<T>(ReadOnlySpan<T> value)
		=> this is SpanParameterDescription<T> { IsReadOnly: true } d &&
		   d.Value.SequenceEqual(value.ToArray());
#endif

	public bool Is<T>(Func<T, bool> comparer)
		=> this is GenericParameterDescription<T> d &&
		   comparer(d.Value);

	public static ParameterDescription FromParameter<T>(T value)
	{
		return new GenericParameterDescription<T>(value, false);
	}

#if FEATURE_SPAN
	public static ParameterDescription FromParameter<T>(Span<T> value)
	{
		return new SpanParameterDescription<T>(value);
	}

	public static ParameterDescription FromParameter<T>(ReadOnlySpan<T> value)
	{
		return new SpanParameterDescription<T>(value);
	}
#endif
	public static ParameterDescription FromOutParameter<T>(T value)
	{
		return new GenericParameterDescription<T>(value, true);
	}

	private class GenericParameterDescription<T> : ParameterDescription
	{
		public T Value { get; }

		public GenericParameterDescription(T value, bool isOutParameter) : base(isOutParameter)
		{
			Value = value;
		}

		/// <inheritdoc />
		public override string? ToString()
		{
			if (Value is string)
			{
				return $"\"{Value}\"";
			}

			return Value?.ToString();
		}
	}

#if FEATURE_SPAN
	private class SpanParameterDescription<T> : ParameterDescription
	{
		public T[] Value { get; }

		public bool IsReadOnly { get; }

		public SpanParameterDescription(Span<T> value) : base(false)
		{
			Value = value.ToArray();
			IsReadOnly = false;
		}

		public SpanParameterDescription(ReadOnlySpan<T> value) : base(false)
		{
			Value = value.ToArray();
			IsReadOnly = true;
		}

		/// <inheritdoc />
		public override string? ToString()
			=> Value?.ToString();
	}
#endif
}

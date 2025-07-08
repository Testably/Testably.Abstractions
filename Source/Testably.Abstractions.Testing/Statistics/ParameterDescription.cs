using System;
using System.Linq;

namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     The description of a parameter in the statistic.
/// </summary>
public abstract class ParameterDescription
{
	/// <summary>
	///     Specifies, if the parameter was used as an <see langword="out" /> parameter.
	/// </summary>
	public bool IsOutParameter { get; }

	/// <summary>
	///     Initializes a new instance of <see cref="ParameterDescription" />.
	/// </summary>
	/// <param name="isOutParameter"></param>
	protected ParameterDescription(bool isOutParameter)
	{
		IsOutParameter = isOutParameter;
	}

	/// <summary>
	///     Creates a <see cref="ParameterDescription" /> from the <paramref name="value" /> used as an <see langword="out" /> parameter.
	/// </summary>
	public static ParameterDescription FromOutParameter<T>(T value)
	{
		return new GenericParameterDescription<T>(value, true);
	}

	/// <summary>
	///     Creates a <see cref="ParameterDescription" /> from the <paramref name="value" />.
	/// </summary>
	public static ParameterDescription FromParameter<T>(T value)
	{
		return new GenericParameterDescription<T>(value, false);
	}

#if FEATURE_SPAN
	/// <summary>
	///     Creates a <see cref="ParameterDescription" /> from the span <paramref name="value" />.
	/// </summary>
	public static ParameterDescription FromParameter<T>(Span<T> value)
	{
		return new SpanParameterDescription<T>(value);
	}
#endif

#if FEATURE_SPAN
	/// <summary>
	///     Creates a <see cref="ParameterDescription" /> from the read-only span <paramref name="value" />.
	/// </summary>
	public static ParameterDescription FromParameter<T>(ReadOnlySpan<T> value)
	{
		return new SpanParameterDescription<T>(value);
	}
#endif

	/// <summary>
	///     Checks, if the value of the parameter equals <paramref name="value" />.
	/// </summary>
	/// <remarks>When the types match, uses <see cref="object.Equals(object)" />.</remarks>
	public bool Is<T>(T value)
		=> this is GenericParameterDescription<T> d &&
		   IsEqual(value, d.Value);

	/// <summary>
	///     Checks, if the sequence of values of the parameter equals <paramref name="value" />.
	/// </summary>
	/// <remarks>
	///     When the types match, uses
	///     <see
	///         cref="Enumerable.SequenceEqual{T}(System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{T})" />
	///     .
	/// </remarks>
	public bool Is<T>(T[] value)
	{
		return this is GenericParameterDescription<T[]> d &&
		       value.SequenceEqual(d.Value);
	}

#if FEATURE_SPAN
	/// <summary>
	///     Checks, if the span value of the parameter equals <paramref name="value" />.
	/// </summary>
	public bool Is<T>(Span<T> value)
		=> this is SpanParameterDescription<T> { IsReadOnly: false } d &&
		   d.Value.SequenceEqual(value.ToArray());
#endif

#if FEATURE_SPAN
	/// <summary>
	///     Checks, if the read-only span value of the parameter equals <paramref name="value" />.
	/// </summary>
	public bool Is<T>(ReadOnlySpan<T> value)
		=> this is SpanParameterDescription<T> { IsReadOnly: true } d &&
		   d.Value.SequenceEqual(value.ToArray());
#endif

#if FEATURE_SPAN
	/// <summary>
	///     Checks, if the value of the parameter equals <paramref name="value" />.
	/// </summary>
	public bool Is<T>(SpanParameterDescription<T> value)
		=> this is SpanParameterDescription<T> d &&
		   d.IsReadOnly == value.IsReadOnly &&
		   d.Value.SequenceEqual(value.Value);
#endif

	/// <summary>
	///     Checks, if the span value of the parameter matches the <paramref name="comparer" />.
	/// </summary>
	public bool Is<T>(Func<T, bool> comparer)
		=> this is GenericParameterDescription<T> d &&
		   comparer(d.Value);

	private static bool IsEqual<T>(T value1, T value2)
	{
		if (value1 is null)
		{
			return value2 is null;
		}

		return value1.Equals(value2);
	}

	private sealed class GenericParameterDescription<T> : ParameterDescription
	{
		public T Value { get; }

		public GenericParameterDescription(T value, bool isOutParameter) : base(isOutParameter)
		{
			Value = value;
		}

		/// <inheritdoc cref="object.ToString()" />
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
	/// <summary>
	/// A parameter description for span values.
	/// </summary>
	public sealed class SpanParameterDescription<T> : ParameterDescription
	{
		/// <summary>
		/// Flag indicating if the span is read-only.
		/// </summary>
		public bool IsReadOnly { get; }
		
		/// <summary>
		/// The values of the span parameter.
		/// </summary>
		public T[] Value { get; }

		/// <summary>
		/// A parameter description for <see cref="Span{T}"/> values.
		/// </summary>
		public SpanParameterDescription(Span<T> value) : base(false)
		{
			Value = value.ToArray();
			IsReadOnly = false;
		}

		/// <summary>
		/// A parameter description for <see cref="ReadOnlySpan{T}"/> values.
		/// </summary>
		public SpanParameterDescription(ReadOnlySpan<T> value) : base(false)
		{
			Value = value.ToArray();
			IsReadOnly = true;
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"[{string.Join(',', Value)}]";
	}
#endif
}

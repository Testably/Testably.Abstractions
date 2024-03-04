using System;

namespace Testably.Abstractions.Testing.Statistics;

public class ParameterDescription<T> : ParameterDescription
{
	public T Value { get; }

	public ParameterDescription(T value, bool isOutParameter) : base(isOutParameter)
	{
		Value = value;
	}

	/// <inheritdoc />
	public override string? ToString()
		=> Value?.ToString();
}

public abstract class ParameterDescription
{
	protected ParameterDescription(bool isOutParameter)
	{
		IsOutParameter = isOutParameter;
	}

	public bool IsOutParameter { get; }

	public bool Is<T>(T value)
		=> this is ParameterDescription<T> d &&
		   (value == null
			   ? d.Value == null
			   : value.Equals(d.Value));

	public bool Is<T>(Func<T, bool> comparer)
		=> this is ParameterDescription<T> d &&
		   comparer(d.Value);

	public static ParameterDescription FromParameter<T>(T value)
	{
		return new ParameterDescription<T>(value, false);
	}

	public static ParameterDescription FromOutParameter<T>(T value)
	{
		return new ParameterDescription<T>(value, true);
	}
}

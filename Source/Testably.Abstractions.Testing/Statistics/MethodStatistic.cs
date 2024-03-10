using System.Linq;

namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Describes a call to a mocked method.
/// </summary>
public sealed class MethodStatistic
{
	/// <summary>
	///     The name of the called method.
	/// </summary>
	public string Name { get; }

	/// <summary>
	///     The parameters of the called method.
	/// </summary>
	public ParameterDescription[] Parameters { get; }

	internal MethodStatistic(string name, ParameterDescription[] parameters)
	{
		Name = name;
		Parameters = parameters;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{Name}({string.Join(",", Parameters.Select(p => p.ToString()))})";
}

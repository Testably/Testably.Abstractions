namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Describes a call to a mocked method.
/// </summary>
public sealed class CallStatistic
{
	/// <summary>
	///     The name of the called method.
	/// </summary>
	public string Name { get; }

	/// <summary>
	///     The parameters of the called method.
	/// </summary>
	public object?[] Parameters { get; }

	internal CallStatistic(string name, object?[] parameters)
	{
		Name = name;
		Parameters = parameters;
	}
}

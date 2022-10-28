namespace Testably.Abstractions;

/// <summary>
///     Allows abstracting time-related system dependencies.
/// </summary>
public interface ITimeSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.DateTime" />.
	/// </summary>
	IDateTime DateTime { get; }

	/// <summary>
	///     Abstractions for <see cref="System.Threading.Tasks.Task" />.
	/// </summary>
	ITask Task { get; }

	/// <summary>
	///     Abstractions for <see cref="System.Threading.Thread" />.
	/// </summary>
	IThread Thread { get; }
}
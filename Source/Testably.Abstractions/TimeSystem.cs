namespace Testably.Abstractions;

/// <summary>
///     Default implementation for time-related system dependencies.
///     <para />
///     Implements <seealso cref="ITimeSystem" />
/// </summary>
public sealed partial class TimeSystem : ITimeSystem
{
	#region ITimeSystem Members

	/// <inheritdoc cref="ITimeSystem.DateTime" />
	public ITimeSystem.IDateTime DateTime
		=> new DateTimeWrapper(this);

	/// <inheritdoc cref="ITimeSystem.Task" />
	public ITimeSystem.ITask Task
		=> new TaskWrapper(this);

	/// <inheritdoc cref="ITimeSystem.Thread" />
	public ITimeSystem.IThread Thread
		=> new ThreadWrapper(this);

	#endregion
}
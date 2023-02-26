using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions;

/// <summary>
///     Default implementation for time-related system dependencies.
///     <para />
///     Implements <seealso cref="ITimeSystem" />
/// </summary>
public sealed class RealTimeSystem : ITimeSystem
{
	#region ITimeSystem Members

	/// <inheritdoc cref="ITimeSystem.DateTime" />
	public IDateTime DateTime
		=> new DateTimeWrapper(this);

	/// <inheritdoc cref="ITimeSystem.Task" />
	public ITask Task
		=> new TaskWrapper(this);

	/// <inheritdoc cref="ITimeSystem.Thread" />
	public IThread Thread
		=> new ThreadWrapper(this);

	#endregion
}

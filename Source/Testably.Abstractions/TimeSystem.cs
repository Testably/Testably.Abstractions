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
        => new DateTimeSystem(this);

    /// <inheritdoc cref="ITimeSystem.Task" />
    public ITimeSystem.ITask Task
        => new TaskSystem(this);

    /// <inheritdoc cref="ITimeSystem.Thread" />
    public ITimeSystem.IThread Thread
        => new ThreadSystem(this);

    #endregion
}
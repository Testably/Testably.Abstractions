namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Contains statistical information about the mock on a given path.
/// </summary>
/// <remarks>
///     See also <seealso cref="IStatistics" /> .
/// </remarks>
public interface IPathStatistics : IStatistics
{
	/// <summary>
	///     Returns the underlying <see cref="IStatistics" /> under <paramref name="path" />.
	/// </summary>
	IStatistics this[string path]
	{
		get;
	}
}

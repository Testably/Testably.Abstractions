namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     The mode of accessing a property (getter or setter).
/// </summary>
public enum PropertyAccess
{
	/// <summary>
	///     The property was read.
	/// </summary>
	Get,

	/// <summary>
	///     The property was written to.
	/// </summary>
	Set,
}

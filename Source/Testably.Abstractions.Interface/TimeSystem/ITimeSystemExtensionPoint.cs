namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Interface to support implementing extension methods on top of nested <see cref="ITimeSystem" /> interfaces.
/// </summary>
public interface ITimeSystemExtensionPoint
{
	/// <summary>
	///     Exposes the underlying time system implementation.
	///     <para />
	///     This is useful for implementing extension methods.
	/// </summary>
	ITimeSystem TimeSystem { get; }
}

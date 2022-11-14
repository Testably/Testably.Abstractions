namespace Testably.Abstractions.RandomSystem;

/// <summary>
///     Interface to support implementing extension methods on top of nested <see cref="IRandomSystem" /> interfaces.
/// </summary>
public interface IRandomSystemExtensionPoint
{
	/// <summary>
	///     Exposes the underlying random system implementation.
	///     <para />
	///     This is useful for implementing extension methods.
	/// </summary>
	IRandomSystem RandomSystem { get; }
}

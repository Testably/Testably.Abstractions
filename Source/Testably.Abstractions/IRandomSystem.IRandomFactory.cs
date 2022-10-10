namespace Testably.Abstractions;

public partial interface IRandomSystem
{
	/// <summary>
	///     Factory for abstracting creation of <see cref="System.Random" />.
	/// </summary>
	interface IRandomFactory : IRandomSystemExtensionPoint
	{
		/// <summary>
		///     Provides a thread-safe <see cref="IRandom" /> instance that may be used concurrently from any thread.
		/// </summary>
		IRandom Shared { get; }

		/// <inheritdoc cref="System.Random()" />
		IRandom New();

		/// <inheritdoc cref="System.Random(int)" />
		IRandom New(int seed);
	}
}
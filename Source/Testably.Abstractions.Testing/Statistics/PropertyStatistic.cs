using System.Globalization;

namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Describes access to a mocked property.
/// </summary>
public sealed class PropertyStatistic
{
	/// <summary>
	///     The global counter value to determine the order of calls.
	/// </summary>
	public int Counter { get; }

	/// <summary>
	///     The name of the accessed property.
	/// </summary>
	public string Name { get; }

	/// <summary>
	///     The mode of the accessed property (getter or setter).
	/// </summary>
	public AccessMode Mode { get; }

	internal PropertyStatistic(int counter, string name, AccessMode mode)
	{
		Counter = counter;
		Name = name;
		Mode = mode;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{Name}{{{Mode.ToString().ToLower(CultureInfo.InvariantCulture)};}}";

	/// <summary>
	///     The mode of accessing a property (getter or setter).
	/// </summary>
	public enum AccessMode
	{
		/// <summary>
		///     The property was read.
		/// </summary>
		Get,

		/// <summary>
		///     The property was written to.
		/// </summary>
		Set
	}
}

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
	public PropertyAccess Access { get; }

	internal PropertyStatistic(int counter, string name, PropertyAccess access)
	{
		Counter = counter;
		Name = name;
		Access = access;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{Name}{{{Access.ToString().ToLower(CultureInfo.InvariantCulture)};}}";
}

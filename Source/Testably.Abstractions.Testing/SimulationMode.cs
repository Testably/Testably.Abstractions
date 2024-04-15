using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing;

/// <summary>
///     The simulation mode for the <see cref="MockFileSystem" />.
/// </summary>
public enum SimulationMode
{
	/// <summary>
	///     Use the underlying operating system (default mode).
	/// </summary>
	Native = 0,

	/// <summary>
	///     Simulates <see cref="OSPlatform.Linux" />.
	/// </summary>
	Linux = 1,

	/// <summary>
	///     Simulates <see cref="OSPlatform.OSX" />.
	/// </summary>
	MacOS = 2,

	/// <summary>
	///     Simulates <see cref="OSPlatform.Windows" />.
	/// </summary>
	Windows = 3
}

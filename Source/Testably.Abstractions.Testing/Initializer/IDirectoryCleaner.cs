using System;

namespace Testably.Abstractions.Testing.Initializer;

/// <summary>
///     Cleans the directory in <see cref="BasePath" /> on dispose.
/// </summary>
public interface IDirectoryCleaner : IDisposable
{
	/// <summary>
	///     The directory that gets cleaned up on dispose.
	/// </summary>
	string BasePath { get; }
}

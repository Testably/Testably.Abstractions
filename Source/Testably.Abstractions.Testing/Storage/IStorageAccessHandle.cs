using System;
using System.IO;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     An access handle that implements <see cref="IDisposable" /> and releases the access lock on dispose.
/// </summary>
internal interface IStorageAccessHandle : IDisposable
{
	/// <summary>
	///     The <see cref="FileAccess" /> that this access handle has.
	/// </summary>
	FileAccess Access { get; }

	/// <summary>
	///     Flag indicating, if the access handle resulted from a deletion request.
	/// </summary>
	bool DeleteAccess { get; }

	/// <summary>
	///     The <see cref="FileShare" /> that this access handle allows.
	/// </summary>
	FileShare Share { get; }
}

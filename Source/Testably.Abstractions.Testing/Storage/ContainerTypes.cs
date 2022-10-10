using System;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     The type of the <see cref="IStorageContainer" />.
/// </summary>
[Flags]
internal enum ContainerTypes
{
	/// <summary>
	///     The <see cref="IStorageContainer" /> is a directory.
	/// </summary>
	Directory = 1,

	/// <summary>
	///     The <see cref="IStorageContainer" /> is a file.
	/// </summary>
	File = 2,

	/// <summary>
	///     The <see cref="IStorageContainer" /> is a directory or a file.
	/// </summary>
	DirectoryOrFile = Directory | File
}
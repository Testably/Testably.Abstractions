using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Determines the entry's type in the <see cref="MockFileSystem" />.
/// </summary>
[Flags]
public enum FileSystemTypes
{
	/// <summary>
	///     The entry is a directory.
	/// </summary>
	Directory = 1,

	/// <summary>
	///     The entry is a file.
	/// </summary>
	File = 2,

	/// <summary>
	///     The entry is a file or directory.
	/// </summary>
	DirectoryOrFile = Directory | File
}

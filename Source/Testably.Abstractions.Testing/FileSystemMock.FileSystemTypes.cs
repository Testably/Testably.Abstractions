using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	/// <summary>
	///     Determines the type of an entry in the <see cref="FileSystemMock" />.
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
}
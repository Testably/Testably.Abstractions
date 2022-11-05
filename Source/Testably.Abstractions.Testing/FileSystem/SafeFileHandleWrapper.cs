using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Wrapper for storing information about a <see cref="SafeFileHandle" />.
/// </summary>
public class SafeFileHandleWrapper
{
	/// <summary>
	///     The path used to open the <see cref="SafeFileHandle" />.
	/// </summary>
	public string Path { get; }

	/// <summary>
	///     The mode used to open the <see cref="SafeFileHandle" />.
	/// </summary>
	public FileMode Mode { get; }

	/// <summary>
	///     The file share used to open the <see cref="SafeFileHandle" />.
	/// </summary>
	public FileShare Share { get; }

	/// <summary>
	///     Initializes a new instance of <see cref="SafeFileHandleWrapper" /> which stores information about a
	///     <see cref="SafeFileHandle" />.
	/// </summary>
	public SafeFileHandleWrapper(string path,
	                             FileMode mode = FileMode.Open,
	                             FileShare share = FileShare.None)
	{
		Path = path;
		Mode = mode;
		Share = share;
	}
}
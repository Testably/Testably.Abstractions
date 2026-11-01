using Microsoft.Win32.SafeHandles;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     The strategy how to deal with <see cref="SafeFileHandle" />s.
/// </summary>
public abstract class SafeFileHandleStrategy
{
	/// <summary>
	///     Maps the <paramref name="fileHandle" /> to a <see cref="SafeFileHandleMock" /> which contains information how this
	///     <see cref="SafeFileHandle" /> should be treated in the <see cref="MockFileSystem" />.
	/// </summary>
	public abstract SafeFileHandleMock MapSafeFileHandle(SafeFileHandle fileHandle);
}

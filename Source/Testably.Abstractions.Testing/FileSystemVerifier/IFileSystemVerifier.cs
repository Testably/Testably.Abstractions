namespace Testably.Abstractions.Testing.FileSystemVerifier;

/// <summary>
///     Verifies the state of the <see cref="IFileSystem" /> in tests.
/// </summary>
public interface IFileSystemVerifier
{
	/// <summary>
	///     <see langword="true" />, if the file or directory exists; otherwise <see langword="false" />.
	/// </summary>
	bool Exists { get; }

	/// <summary>
	///     Specifies if the <see cref="IFileSystemInfo" /> is a <see cref="FileSystemTypes.File" /> or a
	///     <see cref="FileSystemTypes.Directory" />.
	/// </summary>
	/// <remarks>
	///     Set to <see cref="FileSystemTypes.DirectoryOrFile" /> if the path does not exist.
	/// </remarks>
	FileSystemTypes Type { get; }
}

using Testably.Abstractions.Testing.FileSystemVerifier;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Verifies the state of the <see cref="IFileSystem" /> in tests.
/// </summary>
public static class FileSystemVerifierExtensions
{
	/// <summary>
	///     Verify the state of the file or directory at <paramref name="path" /> in the <paramref name="fileSystem" />.
	/// </summary>
	public static IFileSystemVerifier Verify(
		this IFileSystem fileSystem,
		string path)
		=> new FileSystemVerifier.FileSystemVerifier(fileSystem, path);
}

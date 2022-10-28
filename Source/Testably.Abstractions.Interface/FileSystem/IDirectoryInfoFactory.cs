using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

/// <summary>
///     Factory for abstracting the creation of <see cref="DirectoryInfo" />.
/// </summary>
public interface IDirectoryInfoFactory : IFileSystemExtensionPoint
{
	/// <inheritdoc cref="DirectoryInfo(string)" />
	IDirectoryInfo New(string path);

	/// <summary>
	///     Wraps the <paramref name="directoryInfo" /> to the testable interface <see cref="IDirectoryInfo" />.
	/// </summary>
	[return: NotNullIfNotNull("directoryInfo")]
	IDirectoryInfo? Wrap(DirectoryInfo? directoryInfo);
}
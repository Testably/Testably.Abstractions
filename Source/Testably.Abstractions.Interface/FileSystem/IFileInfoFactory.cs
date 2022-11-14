using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

/// <summary>
///     Factory for abstracting the creation of <see cref="FileInfo" />.
/// </summary>
public interface IFileInfoFactory : IFileSystemExtensionPoint
{
	/// <inheritdoc cref="FileInfo(string)" />
	IFileInfo New(string fileName);

	/// <summary>
	///     Wraps the <paramref name="fileInfo" /> to the testable interface <see cref="IFileInfo" />.
	/// </summary>
	[return: NotNullIfNotNull("fileInfo")]
	IFileInfo? Wrap(FileInfo? fileInfo);
}

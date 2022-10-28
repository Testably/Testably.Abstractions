using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

/// <summary>
///     Factory for abstracting the creation of <see cref="System.IO.DirectoryInfo" />.
/// </summary>
public interface IDirectoryInfoFactory : IFileSystemExtensionPoint
{
	/// <inheritdoc cref="System.IO.DirectoryInfo(string)" />
	IDirectoryInfo New(string path);

	/// <summary>
	///     Wraps the <paramref name="directoryInfo" /> to the testable interface <see cref="IDirectoryInfo" />.
	/// </summary>
	[return: NotNullIfNotNull("directoryInfo")]
	IDirectoryInfo? Wrap(DirectoryInfo? directoryInfo);
}
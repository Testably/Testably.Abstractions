using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.FileSystem;

internal sealed class FileStreamWrapper : FileSystemStream, IFileSystemExtensibility
{
	private readonly FileSystemExtensibility _extensibility;

	public FileStreamWrapper(FileStream fileStream)
		: base(fileStream, fileStream.Name, fileStream.IsAsync)

	{
		_extensibility = new FileSystemExtensibility(fileStream);
	}

	/// <inheritdoc cref="IFileSystemExtensibility.TryGetWrappedInstance{T}" />
	public bool TryGetWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
		=> _extensibility.TryGetWrappedInstance(out wrappedInstance);

	/// <inheritdoc cref="StoreMetadata{T}(string, T)" />
	public void StoreMetadata<T>(string key, T? value)
		=> _extensibility.StoreMetadata(key, value);

	/// <inheritdoc cref="RetrieveMetadata{T}(string)" />
	public T? RetrieveMetadata<T>(string key)
		=> _extensibility.RetrieveMetadata<T>(key);
}

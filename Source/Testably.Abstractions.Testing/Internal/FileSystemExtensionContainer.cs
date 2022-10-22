using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions.Testing.Internal;

internal class FileSystemExtensionContainer : IFileSystem.IFileSystemExtensionContainer
{
	private readonly Dictionary<string, object?> _metadata = new();

	/// <inheritdoc cref="IFileSystem.IFileSystemExtensionContainer.HasWrappedInstance{T}(out T)" />
	public bool HasWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
	{
		wrappedInstance = default;
		return false;
	}

	/// <inheritdoc />
	public void StoreMetadata<T>(string key, T? value)
	{
		_metadata[key] = value;
	}

	/// <inheritdoc />
	public T? RetrieveMetadata<T>(string key)
	{
		if (_metadata.TryGetValue(key, out object? value) &&
		    value is T?)
		{
			return (T?)value;
		}

		return default;
	}

	internal void CopyMetadataTo(IFileSystem.IFileSystemExtensionContainer target)
	{
		if (target is FileSystemExtensionContainer targetContainer)
		{
			foreach (KeyValuePair<string, object?> item in _metadata)
			{
				targetContainer._metadata[item.Key] = item.Value;
			}
		}
	}
}
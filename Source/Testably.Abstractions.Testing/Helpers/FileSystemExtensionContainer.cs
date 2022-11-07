using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Testing.Helpers;

internal class FileSystemExtensionContainer : IFileSystemExtensionContainer
{
	private readonly Dictionary<string, object?> _metadata = new();

	/// <inheritdoc cref="IFileSystemExtensionContainer.HasWrappedInstance{T}(out T)" />
	public bool HasWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
	{
		wrappedInstance = default;
		return false;
	}

	/// <inheritdoc cref="IFileSystemExtensionContainer.StoreMetadata{T}(string, T)" />
	public void StoreMetadata<T>(string key, T? value)
	{
		_metadata[key] = value;
	}

	/// <inheritdoc cref="IFileSystemExtensionContainer.RetrieveMetadata{T}(string)" />
	public T? RetrieveMetadata<T>(string key)
	{
		if (_metadata.TryGetValue(key, out object? value) &&
		    // ReSharper disable once MergeCastWithTypeCheck -- Not possible due to nullable
		    value is T?)
		{
			return (T?)value;
		}

		return default;
	}

	internal void CopyMetadataTo(IFileSystemExtensionContainer target)
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions.FileSystem;

internal class FileSystemExtensionContainer : IFileSystemExtensionContainer
{
	private readonly object _wrappedInstance;
	private readonly Dictionary<string, object?> _metadata = new();

	public FileSystemExtensionContainer(object wrappedInstance)
	{
		_wrappedInstance = wrappedInstance;
	}

	/// <inheritdoc cref="IFileSystemExtensionContainer.HasWrappedInstance{T}(out T)" />
	public bool HasWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
	{
		// ReSharper disable once MergeCastWithTypeCheck -- Not possible due to nullable
		wrappedInstance = _wrappedInstance is T?
			? (T?)_wrappedInstance
			: default;
		return !Equals(wrappedInstance, default(T));
	}

	/// <inheritdoc cref="StoreMetadata{T}(string, T?)" />
	public void StoreMetadata<T>(string key, T? value)
	{
		_metadata[key] = value;
	}

	/// <inheritdoc cref="RetrieveMetadata{T}(string)" />
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
}
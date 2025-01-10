using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed class FileSystemExtensibility : IFileSystemExtensibility
{
	private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);

	#region IFileSystemExtensibility Members

	/// <inheritdoc cref="IFileSystemExtensibility.RetrieveMetadata{T}(string)" />
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

	/// <inheritdoc cref="IFileSystemExtensibility.StoreMetadata{T}(string, T)" />
	public void StoreMetadata<T>(string key, T? value)
	{
		_metadata[key] = value;
	}

	/// <inheritdoc cref="IFileSystemExtensibility.TryGetWrappedInstance{T}" />
	public bool TryGetWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
	{
		wrappedInstance = default;
		return false;
	}

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		if (_metadata.Count == 0)
		{
			return "[]";
		}

		return
			$"[{string.Join(", ", _metadata.Select(x => $"{x.Key}: {x.Value}"))}]";
	}

	internal void CopyMetadataTo(IFileSystemExtensibility target)
	{
		if (target is FileSystemExtensibility targetContainer)
		{
			foreach (KeyValuePair<string, object?> item in _metadata)
			{
				targetContainer._metadata[item.Key] = item.Value;
			}
		}
	}
}

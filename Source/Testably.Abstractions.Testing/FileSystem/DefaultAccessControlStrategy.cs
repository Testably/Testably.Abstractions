using System;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Default implementation of an <see cref="IAccessControlStrategy" /> which uses a callback to determine if access
///     should be granted.
/// </summary>
public class DefaultAccessControlStrategy : IAccessControlStrategy
{
	private readonly Func<string, IFileSystemExtensibility, bool> _callback;

	/// <summary>
	///     Initializes a new instance of <see cref="DefaultAccessControlStrategy" /> which takes a callback to determine if
	///     access should be granted.
	/// </summary>
	public DefaultAccessControlStrategy(
		Func<string, IFileSystemExtensibility, bool> callback)
	{
		_callback = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	/// <inheritdoc cref="IAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility)" />
	public bool IsAccessGranted(string fullPath,
		IFileSystemExtensibility extensibility)
		=> _callback(fullPath, extensibility);
}

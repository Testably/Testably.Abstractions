using System;
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Default implementation of an <see cref="AccessControlStrategy" /> which uses a callback to determine if access
///     should be granted.
/// </summary>
public class DefaultAccessControlStrategy : AccessControlStrategy
{
	private readonly Func<string, IFileSystemExtensibility, FileAccess, bool> _callback;

	/// <summary>
	///     Initializes a new instance of <see cref="DefaultAccessControlStrategy" /> which takes a callback to determine if
	///     access should be granted.
	/// </summary>
	public DefaultAccessControlStrategy(
		Func<string, IFileSystemExtensibility, bool> callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException(nameof(callback));
		}

		_callback = (fullPath, extensibility, _) => callback(fullPath, extensibility);
	}

	/// <summary>
	///     Initializes a new instance of <see cref="DefaultAccessControlStrategy" /> which takes a callback to determine if
	///     the requested <see cref="FileAccess" /> should be granted.
	/// </summary>
	public DefaultAccessControlStrategy(
		Func<string, IFileSystemExtensibility, FileAccess, bool> callback)
	{
		_callback = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	/// <inheritdoc cref="AccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility, FileAccess)" />
	public override bool IsAccessGranted(string fullPath,
		IFileSystemExtensibility extensibility,
		FileAccess requestedAccess)
		=> _callback(fullPath, extensibility, requestedAccess);
}

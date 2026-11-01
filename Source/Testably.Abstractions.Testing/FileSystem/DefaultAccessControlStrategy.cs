using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Default implementation of an <see cref="AccessControlStrategy" /> which uses a callback to determine if access
///     should be granted.
/// </summary>
public class DefaultAccessControlStrategy : AccessControlStrategy
{
	private readonly Func<string, IFileSystemExtensibility, AccessRequest, bool> _callback;

	/// <summary>
	///     Initializes a new instance of <see cref="DefaultAccessControlStrategy" /> which takes a callback to determine if
	///     the <see cref="AccessRequest" /> should be granted.
	/// </summary>
	public DefaultAccessControlStrategy(
		Func<string, IFileSystemExtensibility, AccessRequest, bool> callback)
	{
		_callback = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	/// <inheritdoc cref="AccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility, AccessRequest)" />
	public override bool IsAccessGranted(string fullPath,
		IFileSystemExtensibility extensibility,
		AccessRequest request)
		=> _callback(fullPath, extensibility, request);
}

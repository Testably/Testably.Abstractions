using Microsoft.Win32.SafeHandles;
using System;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Default implementation of a <see cref="SafeFileHandleStrategy" /> which uses a callback for mapping
///     <see cref="SafeFileHandle" />s.
/// </summary>
public class DefaultSafeFileHandleStrategy : SafeFileHandleStrategy
{
	private readonly Func<SafeFileHandle, SafeFileHandleMock> _callback;

	/// <summary>
	///     Initializes a new instance of <see cref="DefaultSafeFileHandleStrategy" /> which takes a
	///     <paramref name="callback" /> to perform the mapping from <see cref="SafeFileHandle" /> to
	///     <see cref="SafeFileHandleMock" />.
	/// </summary>
	public DefaultSafeFileHandleStrategy(
		Func<SafeFileHandle, SafeFileHandleMock> callback)
	{
		_callback = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	/// <inheritdoc cref="SafeFileHandleStrategy.MapSafeFileHandle(SafeFileHandle)" />
#if NET6_0_OR_GREATER
	[ExcludeFromCodeCoverage(Justification = "SafeFileHandle cannot be unit tested.")]
#endif
	public override SafeFileHandleMock MapSafeFileHandle(SafeFileHandle fileHandle)
		=> _callback.Invoke(fileHandle);
}

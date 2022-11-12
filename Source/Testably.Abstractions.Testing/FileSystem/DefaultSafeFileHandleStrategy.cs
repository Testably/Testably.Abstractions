﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Default implementation of an <see cref="ISafeFileHandleStrategy" /> which uses a callback for mapping
///     <see cref="SafeFileHandle" />s.
/// </summary>
public class DefaultSafeFileHandleStrategy : ISafeFileHandleStrategy
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

	/// <inheritdoc cref="ISafeFileHandleStrategy.MapSafeFileHandle(SafeFileHandle)" />
#if NET6_0_OR_GREATER
	[ExcludeFromCodeCoverage(Justification = "SafeFileHandle cannot be unit tested.")]
#endif
	public SafeFileHandleMock MapSafeFileHandle(SafeFileHandle fileHandle)
		=> _callback.Invoke(fileHandle);
}
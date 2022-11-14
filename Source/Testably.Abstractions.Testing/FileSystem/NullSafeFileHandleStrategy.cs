using Microsoft.Win32.SafeHandles;
using System.Diagnostics.CodeAnalysis;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="ISafeFileHandleStrategy" /> which throws an exception.
/// </summary>
public class NullSafeFileHandleStrategy : ISafeFileHandleStrategy
{
	/// <inheritdoc cref="ISafeFileHandleStrategy.MapSafeFileHandle(SafeFileHandle)" />
#if NET6_0_OR_GREATER
	[ExcludeFromCodeCoverage(Justification = "SafeFileHandle cannot be unit tested.")]
#endif
	public SafeFileHandleMock MapSafeFileHandle(SafeFileHandle fileHandle)
	{
		if (fileHandle.IsInvalid)
		{
			throw ExceptionFactory.HandleIsInvalid();
		}

		throw ExceptionFactory.NotSupportedSafeFileHandle();
	}
}

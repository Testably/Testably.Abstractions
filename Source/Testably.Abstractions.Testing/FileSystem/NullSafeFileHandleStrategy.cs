using Microsoft.Win32.SafeHandles;
using Testably.Abstractions.Testing.Helpers;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of a <see cref="SafeFileHandleStrategy" /> which throws an exception.
/// </summary>
public class NullSafeFileHandleStrategy : SafeFileHandleStrategy
{
	/// <inheritdoc cref="SafeFileHandleStrategy.MapSafeFileHandle(SafeFileHandle)" />
#if NET6_0_OR_GREATER
	[ExcludeFromCodeCoverage(Justification = "SafeFileHandle cannot be unit tested.")]
#endif
	public override SafeFileHandleMock MapSafeFileHandle(SafeFileHandle fileHandle)
	{
		if (fileHandle.IsInvalid)
		{
			throw ExceptionFactory.HandleIsInvalid();
		}

		throw ExceptionFactory.NotSupportedSafeFileHandle();
	}
}

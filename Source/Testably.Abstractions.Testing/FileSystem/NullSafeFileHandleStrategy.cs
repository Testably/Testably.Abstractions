using Microsoft.Win32.SafeHandles;
using Testably.Abstractions.Testing.Helpers;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="ISafeFileHandleStrategy" /> which throws an exception.
/// </summary>
public class NullSafeFileHandleStrategy : ISafeFileHandleStrategy
{
	#region ISafeFileHandleStrategy Members

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

	#endregion
}

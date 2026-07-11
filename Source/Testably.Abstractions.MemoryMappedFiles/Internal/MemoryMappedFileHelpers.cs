using System;
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Internal;

internal static class MemoryMappedFileHelpers
{
	/// <summary>
	///     Retrieves the <see cref="IFileSystemExtensibility" /> from the <paramref name="fileStream" />
	///     or throws a <see cref="NotSupportedException" /> if it is not supported.
	/// </summary>
	public static IFileSystemExtensibility GetExtensibilityOrThrow(
		this FileSystemStream fileStream)
		=> fileStream as IFileSystemExtensibility
		   ?? throw new NotSupportedException(
			   $"{fileStream.GetType()} does not support IFileSystemExtensibility.");

	/// <summary>
	///     Returns <see langword="true" /> when the <paramref name="fileSystem" /> is the real file
	///     system (and therefore has an underlying operating-system file system to delegate to).
	/// </summary>
	/// <remarks>
	///     This is detected without touching the disk by checking whether a newly created
	///     <see cref="IFileInfo" /> wraps a real <see cref="FileInfo" />.
	/// </remarks>
	public static bool IsRealFileSystem(this IFileSystem fileSystem)
		=> fileSystem.FileInfo.New("memory-mapped-file-probe") is IFileSystemExtensibility
			   extensibility &&
		   extensibility.TryGetWrappedInstance(out FileInfo? _);
}

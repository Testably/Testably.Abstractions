using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_0 || NETSTANDARD2_1
using System.Linq;
using System.Reflection;
#endif
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Internal;

internal static class MemoryMappedFileHelpers
{
	/// <summary>
	///     Returns the stride between two elements of an array of <typeparamref name="T" />,
	///     matching the aligned size used by the BCL <c>UnmanagedMemoryAccessor</c> array
	///     operations: sizes 1 and 2 are kept, larger sizes are rounded up to a multiple of 4.
	/// </summary>
	public static int AlignedSizeOf<T>() where T : struct
	{
		int size = Unsafe.SizeOf<T>();
		if (size is 1 or 2)
		{
			return size;
		}

		return (size + 3) & ~3;
	}

	/// <summary>
	///     Throws an <see cref="ArgumentException" /> when <typeparamref name="T" /> is a struct
	///     containing object references, matching the BCL <c>UnmanagedMemoryAccessor</c>, which
	///     never reinterprets raw bytes as references.
	/// </summary>
	public static void ThrowIfContainsReferences<T>() where T : struct
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (ReferenceCheck<T>.ContainsReferences)
#else
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
#endif
		{
			#pragma warning disable MA0015 // Matches the parameter-less BCL message for a reference-containing struct.
			throw new ArgumentException(
				"The specified Type must be a struct containing no references.");
			#pragma warning restore MA0015
		}
	}

#if NETSTANDARD2_0 || NETSTANDARD2_1
	private static class ReferenceCheck<T> where T : struct
	{
		public static readonly bool ContainsReferences = Check(typeof(T));

		private static bool Check(Type type)
			=> type
				.GetFields(BindingFlags.Instance | BindingFlags.Public |
				           BindingFlags.NonPublic)
				.Any(field
					=> (!field.FieldType.IsValueType && !field.FieldType.IsPointer) ||
					   (field.FieldType.IsValueType && field.FieldType != type &&
					    Check(field.FieldType)));
	}
#endif

	/// <summary>
	///     Returns whether a view with the given <paramref name="access" /> supports reading.
	/// </summary>
	public static bool SupportsReading(this MemoryMappedFileAccess access)
		=> access is not MemoryMappedFileAccess.Write;

	/// <summary>
	///     Returns whether a view with the given <paramref name="access" /> supports writing.
	/// </summary>
	public static bool SupportsWriting(this MemoryMappedFileAccess access)
		=> access is not (MemoryMappedFileAccess.Read
			or MemoryMappedFileAccess.ReadExecute);

	/// <summary>
	///     Throws an <see cref="ArgumentOutOfRangeException" /> when the <paramref name="access" />
	///     is not a defined <see cref="MemoryMappedFileAccess" /> value, matching the BCL.
	/// </summary>
	public static void ThrowIfOutOfRange(this MemoryMappedFileAccess access,
		string paramName)
	{
		if (access < MemoryMappedFileAccess.ReadWrite ||
		    access > MemoryMappedFileAccess.ReadWriteExecute)
		{
			throw new ArgumentOutOfRangeException(paramName);
		}
	}

	/// <summary>
	///     Throws an <see cref="ArgumentOutOfRangeException" /> when the <paramref name="value" />
	///     is negative, matching the message of the BCL argument validation.
	/// </summary>
	public static void ThrowIfNegative(long value, string paramName)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(paramName, value,
				$"{paramName} ('{value}') must be a non-negative value.");
		}
	}

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

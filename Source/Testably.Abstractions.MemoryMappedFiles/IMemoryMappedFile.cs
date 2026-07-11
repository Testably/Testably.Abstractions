using System;
using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

/// <inheritdoc cref="MemoryMappedFile" />
public interface IMemoryMappedFile : IFileSystemEntity, IDisposable
{
	/// <inheritdoc cref="MemoryMappedFile.CreateViewAccessor()" />
	IMemoryMappedViewAccessor CreateViewAccessor();

	/// <inheritdoc cref="MemoryMappedFile.CreateViewAccessor(long, long)" />
	IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size);

	/// <inheritdoc cref="MemoryMappedFile.CreateViewAccessor(long, long, MemoryMappedFileAccess)" />
	IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size,
		MemoryMappedFileAccess access);

	/// <inheritdoc cref="MemoryMappedFile.CreateViewStream()" />
	MemoryMappedFileSystemViewStream CreateViewStream();

	/// <inheritdoc cref="MemoryMappedFile.CreateViewStream(long, long)" />
	MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size);

	/// <inheritdoc cref="MemoryMappedFile.CreateViewStream(long, long, MemoryMappedFileAccess)" />
	MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size,
		MemoryMappedFileAccess access);
}

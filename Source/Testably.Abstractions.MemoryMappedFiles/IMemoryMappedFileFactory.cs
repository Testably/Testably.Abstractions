using System.IO;
using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

/// <inheritdoc cref="MemoryMappedFile" />
public interface IMemoryMappedFileFactory : IFileSystemEntity
{
	/// <inheritdoc cref="MemoryMappedFile.CreateFromFile(string)" />
	IMemoryMappedFile CreateFromFile(string path);

	/// <inheritdoc cref="MemoryMappedFile.CreateFromFile(string, FileMode)" />
	IMemoryMappedFile CreateFromFile(string path, FileMode mode);

	/// <inheritdoc cref="MemoryMappedFile.CreateFromFile(string, FileMode, string)" />
	IMemoryMappedFile CreateFromFile(string path, FileMode mode, string? mapName);

	/// <inheritdoc cref="MemoryMappedFile.CreateFromFile(string, FileMode, string, long)" />
	IMemoryMappedFile CreateFromFile(string path, FileMode mode, string? mapName,
		long capacity);

	/// <inheritdoc cref="MemoryMappedFile.CreateFromFile(string, FileMode, string, long, MemoryMappedFileAccess)" />
	IMemoryMappedFile CreateFromFile(string path, FileMode mode, string? mapName,
		long capacity, MemoryMappedFileAccess access);

	/// <inheritdoc cref="MemoryMappedFile.CreateFromFile(FileStream, string, long, MemoryMappedFileAccess, HandleInheritability, bool)" />
	/// <remarks>
	///     In this abstraction the file is provided as a <see cref="FileSystemStream" /> instead of a
	///     <see cref="FileStream" />, so it works against both the real and the mocked file system.
	/// </remarks>
	IMemoryMappedFile CreateFromFile(FileSystemStream fileStream, string? mapName,
		long capacity, MemoryMappedFileAccess access, HandleInheritability inheritability,
		bool leaveOpen);

	/// <inheritdoc cref="MemoryMappedFile.CreateNew(string, long)" />
	/// <remarks>
	///     This creates operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there. On the real file system it forwards to
	///     <see cref="MemoryMappedFile.CreateNew(string, long)" />.
	/// </remarks>
	IMemoryMappedFile CreateNew(string? mapName, long capacity);

	/// <inheritdoc cref="MemoryMappedFile.CreateNew(string, long, MemoryMappedFileAccess)" />
	/// <remarks>
	///     This creates operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	IMemoryMappedFile CreateNew(string? mapName, long capacity,
		MemoryMappedFileAccess access);

	/// <inheritdoc cref="MemoryMappedFile.CreateNew(string, long, MemoryMappedFileAccess, MemoryMappedFileOptions, HandleInheritability)" />
	/// <remarks>
	///     This creates operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	IMemoryMappedFile CreateNew(string? mapName, long capacity,
		MemoryMappedFileAccess access, MemoryMappedFileOptions options,
		HandleInheritability inheritability);

	/// <inheritdoc cref="MemoryMappedFile.CreateOrOpen(string, long)" />
	/// <remarks>
	///     This creates or opens operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	[SupportedOSPlatform("windows")]
	IMemoryMappedFile CreateOrOpen(string mapName, long capacity);

	/// <inheritdoc cref="MemoryMappedFile.CreateOrOpen(string, long, MemoryMappedFileAccess)" />
	/// <remarks>
	///     This creates or opens operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	[SupportedOSPlatform("windows")]
	IMemoryMappedFile CreateOrOpen(string mapName, long capacity,
		MemoryMappedFileAccess access);

	/// <inheritdoc cref="MemoryMappedFile.CreateOrOpen(string, long, MemoryMappedFileAccess, MemoryMappedFileOptions, HandleInheritability)" />
	/// <remarks>
	///     This creates or opens operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	[SupportedOSPlatform("windows")]
	IMemoryMappedFile CreateOrOpen(string mapName, long capacity,
		MemoryMappedFileAccess access, MemoryMappedFileOptions options,
		HandleInheritability inheritability);

	/// <inheritdoc cref="MemoryMappedFile.OpenExisting(string)" />
	/// <remarks>
	///     This opens existing operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	[SupportedOSPlatform("windows")]
	IMemoryMappedFile OpenExisting(string mapName);

	/// <inheritdoc cref="MemoryMappedFile.OpenExisting(string, MemoryMappedFileRights)" />
	/// <remarks>
	///     This opens existing operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	[SupportedOSPlatform("windows")]
	IMemoryMappedFile OpenExisting(string mapName,
		MemoryMappedFileRights desiredAccessRights);

	/// <inheritdoc cref="MemoryMappedFile.OpenExisting(string, MemoryMappedFileRights, HandleInheritability)" />
	/// <remarks>
	///     This opens existing operating-system shared memory that is not backed by a file, so it is
	///     <b>not supported</b> on the <c>MockFileSystem</c> and throws a
	///     <see cref="System.NotSupportedException" /> there.
	/// </remarks>
	[SupportedOSPlatform("windows")]
	IMemoryMappedFile OpenExisting(string mapName,
		MemoryMappedFileRights desiredAccessRights, HandleInheritability inheritability);
}

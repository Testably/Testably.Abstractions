using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

internal sealed class MemoryMappedFileFactory(IFileSystem fileSystem) : IMemoryMappedFileFactory
{
	#region IMemoryMappedFileFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; } = fileSystem;

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateFromFile(string)" />
	public IMemoryMappedFile CreateFromFile(string path)
		=> CreateFromFile(path, FileMode.Open, null, 0,
			MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateFromFile(string, FileMode)" />
	public IMemoryMappedFile CreateFromFile(string path, FileMode mode)
		=> CreateFromFile(path, mode, null, 0, MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateFromFile(string, FileMode, string)" />
	public IMemoryMappedFile CreateFromFile(string path, FileMode mode, string? mapName)
		=> CreateFromFile(path, mode, mapName, 0, MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateFromFile(string, FileMode, string, long)" />
	public IMemoryMappedFile CreateFromFile(string path, FileMode mode, string? mapName,
		long capacity)
		=> CreateFromFile(path, mode, mapName, capacity,
			MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateFromFile(string, FileMode, string, long, MemoryMappedFileAccess)" />
	public IMemoryMappedFile CreateFromFile(string path, FileMode mode, string? mapName,
		long capacity, MemoryMappedFileAccess access)
	{
		// The argument validation order matches the BCL: map name, capacity, access range,
		// mode, write access.
		ValidateMapName(mapName);
		ValidateCapacity(capacity);
		access.ThrowIfOutOfRange(nameof(access));
		if (mode == FileMode.Append)
		{
			throw new ArgumentException(
				"FileMode.Append is not permitted when creating new memory mapped files. Instead, use FileMode.OpenOrCreate.",
				nameof(mode));
		}

		if (mode == FileMode.Truncate)
		{
			throw new ArgumentException(
				"FileMode.Truncate is not permitted when creating new memory mapped files.",
				nameof(mode));
		}

		ThrowIfWriteAccess(access);

		bool fileExisted = mode == FileMode.Open || FileSystem.File.Exists(path);
		FileSystemStream stream =
			FileSystem.FileStream.New(path, mode, ToFileAccess(access), PathBasedFileShare);
		try
		{
			IFileSystemExtensibility extensibility = stream.GetExtensibilityOrThrow();
			if (extensibility.TryGetWrappedInstance(out FileStream? realStream))
			{
				MemoryMappedFile instance = MemoryMappedFile.CreateFromFile(
					realStream, mapName, capacity, access, HandleInheritability.None,
					leaveOpen: true);
				return new MemoryMappedFileWrapper(FileSystem, instance, stream);
			}

			ThrowIfMapNamed(mapName);
			return new MemoryMappedFileMock(FileSystem, stream, capacity, access,
				ownsStream: true);
		}
		catch
		{
			stream.Dispose();
			// The BCL deletes a file that was only created by this call when the creation of
			// the memory-mapped file fails afterwards.
			if (!fileExisted)
			{
				FileSystem.File.Delete(path);
			}

			throw;
		}
	}

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateFromFile(FileSystemStream, string, long, MemoryMappedFileAccess, HandleInheritability, bool)" />
	public IMemoryMappedFile CreateFromFile(FileSystemStream fileStream, string? mapName,
		long capacity, MemoryMappedFileAccess access, HandleInheritability inheritability,
		bool leaveOpen)
	{
		if (fileStream == null)
		{
			throw new ArgumentNullException(nameof(fileStream));
		}

		// The argument validation order matches the BCL: map name, capacity, access, empty
		// file, inheritability.
		ValidateMapName(mapName);
		ValidateCapacity(capacity);
		access.ThrowIfOutOfRange(nameof(access));
		ThrowIfWriteAccess(access);
		MemoryMappedFileHelpers.ThrowIfEmptyFileWithZeroCapacity(capacity, fileStream.Length);
		ValidateInheritability(inheritability);
		IFileSystemExtensibility extensibility = fileStream.GetExtensibilityOrThrow();
		if (extensibility.TryGetWrappedInstance(out FileStream? realStream))
		{
			MemoryMappedFile instance = MemoryMappedFile.CreateFromFile(
				realStream, mapName, capacity, access, inheritability, leaveOpen);
			return new MemoryMappedFileWrapper(FileSystem, instance, backingStream: null);
		}

		ThrowIfMapNamed(mapName);
		return new MemoryMappedFileMock(FileSystem, fileStream, capacity, access,
			ownsStream: !leaveOpen);
	}

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateNew(string, long)" />
	public IMemoryMappedFile CreateNew(string? mapName, long capacity)
		=> Forward(() => MemoryMappedFile.CreateNew(mapName, capacity));

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateNew(string, long, MemoryMappedFileAccess)" />
	public IMemoryMappedFile CreateNew(string? mapName, long capacity,
		MemoryMappedFileAccess access)
		=> Forward(() => MemoryMappedFile.CreateNew(mapName, capacity, access));

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateNew(string, long, MemoryMappedFileAccess, MemoryMappedFileOptions, HandleInheritability)" />
	public IMemoryMappedFile CreateNew(string? mapName, long capacity,
		MemoryMappedFileAccess access, MemoryMappedFileOptions options,
		HandleInheritability inheritability)
		=> Forward(() => MemoryMappedFile.CreateNew(mapName, capacity, access, options,
			inheritability));

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateOrOpen(string, long)" />
	[SupportedOSPlatform("windows")]
	public IMemoryMappedFile CreateOrOpen(string mapName, long capacity)
		=> Forward(() => MemoryMappedFile.CreateOrOpen(mapName, capacity));

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateOrOpen(string, long, MemoryMappedFileAccess)" />
	[SupportedOSPlatform("windows")]
	public IMemoryMappedFile CreateOrOpen(string mapName, long capacity,
		MemoryMappedFileAccess access)
		=> Forward(() => MemoryMappedFile.CreateOrOpen(mapName, capacity, access));

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateOrOpen(string, long, MemoryMappedFileAccess, MemoryMappedFileOptions, HandleInheritability)" />
	[SupportedOSPlatform("windows")]
	public IMemoryMappedFile CreateOrOpen(string mapName, long capacity,
		MemoryMappedFileAccess access, MemoryMappedFileOptions options,
		HandleInheritability inheritability)
		=> Forward(() => MemoryMappedFile.CreateOrOpen(mapName, capacity, access, options,
			inheritability));

	/// <inheritdoc cref="IMemoryMappedFileFactory.OpenExisting(string)" />
	[SupportedOSPlatform("windows")]
	public IMemoryMappedFile OpenExisting(string mapName)
		=> Forward(() => MemoryMappedFile.OpenExisting(mapName));

	/// <inheritdoc cref="IMemoryMappedFileFactory.OpenExisting(string, MemoryMappedFileRights)" />
	[SupportedOSPlatform("windows")]
	public IMemoryMappedFile OpenExisting(string mapName,
		MemoryMappedFileRights desiredAccessRights)
		=> Forward(() => MemoryMappedFile.OpenExisting(mapName, desiredAccessRights));

	/// <inheritdoc cref="IMemoryMappedFileFactory.OpenExisting(string, MemoryMappedFileRights, HandleInheritability)" />
	[SupportedOSPlatform("windows")]
	public IMemoryMappedFile OpenExisting(string mapName,
		MemoryMappedFileRights desiredAccessRights, HandleInheritability inheritability)
		=> Forward(() => MemoryMappedFile.OpenExisting(mapName, desiredAccessRights,
			inheritability));

	#endregion

	private MemoryMappedFileWrapper Forward(Func<MemoryMappedFile> onRealFileSystem)
	{
		if (!FileSystem.IsRealFileSystem())
		{
			throw new NotSupportedException(
				"Named or anonymous memory-mapped files that are not backed by a file are not supported on the mocked file system.");
		}

		return new MemoryMappedFileWrapper(FileSystem, onRealFileSystem(),
			backingStream: null);
	}

	private static FileAccess ToFileAccess(MemoryMappedFileAccess access)
	{
#if NETSTANDARD2_0
		if (access == MemoryMappedFileAccess.CopyOnWrite && IsNetFramework)
		{
			// On .NET Framework the BCL opens the file of a copy-on-write mapping with read
			// access only, so e.g. a read-only file can be mapped copy-on-write; modern .NET
			// requests read-write access instead.
			return FileAccess.Read;
		}
#endif
		return access.SupportsWriting()
			? FileAccess.ReadWrite
			: FileAccess.Read;
	}

#if NETSTANDARD2_0
	private static readonly bool IsNetFramework =
		System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
			.StartsWith(".NET Framework", StringComparison.Ordinal);

	private static readonly FileShare PathBasedFileShare =
		IsNetFramework ? FileShare.None : FileShare.Read;
#else
	private const FileShare PathBasedFileShare = FileShare.Read;
#endif

	private static void ThrowIfMapNamed(string? mapName)
	{
		if (mapName != null)
		{
			throw new NotSupportedException(
				"Named memory-mapped files are not supported on the mocked file system.");
		}
	}

	private static void ThrowIfWriteAccess(MemoryMappedFileAccess access)
	{
		if (access == MemoryMappedFileAccess.Write)
		{
			throw new ArgumentException(
				"MemoryMappedFileAccess.Write is not permitted when creating new memory mapped files. Use MemoryMappedFileAccess.ReadWrite instead.",
				nameof(access));
		}
	}

	private static void ValidateInheritability(HandleInheritability inheritability)
	{
		if (inheritability is < HandleInheritability.None or > HandleInheritability.Inheritable)
		{
			throw new ArgumentOutOfRangeException(nameof(inheritability));
		}
	}

	private static void ValidateCapacity(long capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity), capacity,
				"The capacity must be greater than or equal to 0. 0 represents the size of the file being mapped.");
		}
	}

	private static void ValidateMapName(string? mapName)
	{
		if (mapName is { Length: 0, })
		{
			#pragma warning disable MA0015 // Matches the parameter-less BCL message for an empty map name.
			throw new ArgumentException("Map name cannot be an empty string.");
			#pragma warning restore MA0015
		}
	}
}

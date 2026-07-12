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
		ValidateMapName(mapName);
		ValidateAccess(access);
		ValidateCapacity(capacity);
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

		bool fileExisted = mode == FileMode.Open || FileSystem.File.Exists(path);
		FileSystemStream stream =
			FileSystem.FileStream.New(path, mode, ToFileAccess(access));
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
		ValidateMapName(mapName);
		ValidateAccess(access);
		ValidateCapacity(capacity);
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
		=> access.SupportsWriting()
			? FileAccess.ReadWrite
			: FileAccess.Read;

	private static void ThrowIfMapNamed(string? mapName)
	{
		if (mapName != null)
		{
			throw new NotSupportedException(
				"Named memory-mapped files are not supported on the mocked file system.");
		}
	}

	private static void ValidateAccess(MemoryMappedFileAccess access)
	{
		access.ThrowIfOutOfRange(nameof(access));

		if (access == MemoryMappedFileAccess.Write)
		{
			throw new ArgumentException(
				"MemoryMappedFileAccess.Write is not permitted when creating new memory mapped files. Use MemoryMappedFileAccess.ReadWrite instead.",
				nameof(access));
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

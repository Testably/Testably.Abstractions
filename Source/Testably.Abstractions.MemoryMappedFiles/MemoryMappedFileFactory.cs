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

			return new MemoryMappedFileMock(FileSystem, stream, capacity, access,
				ownsStream: true);
		}
		catch
		{
			stream.Dispose();
			throw;
		}
	}

	/// <inheritdoc cref="IMemoryMappedFileFactory.CreateFromFile(FileSystemStream, string, long, MemoryMappedFileAccess, HandleInheritability, bool)" />
	public IMemoryMappedFile CreateFromFile(FileSystemStream fileStream, string? mapName,
		long capacity, MemoryMappedFileAccess access, HandleInheritability inheritability,
		bool leaveOpen)
	{
		IFileSystemExtensibility extensibility = fileStream.GetExtensibilityOrThrow();
		if (extensibility.TryGetWrappedInstance(out FileStream? realStream))
		{
			MemoryMappedFile instance = MemoryMappedFile.CreateFromFile(
				realStream, mapName, capacity, access, inheritability, leaveOpen);
			return new MemoryMappedFileWrapper(FileSystem, instance, backingStream: null);
		}

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
		=> access is MemoryMappedFileAccess.Read or MemoryMappedFileAccess.ReadExecute
			? FileAccess.Read
			: FileAccess.ReadWrite;
}

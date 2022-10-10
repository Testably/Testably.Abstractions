using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;
using static Testably.Abstractions.Testing.FileSystemMock;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     The container storing the current data of the <see cref="IFileSystem" /> in memory.
/// </summary>
internal sealed class InMemoryStorage : IStorage
{
	private readonly ConcurrentDictionary<IStorageLocation, IStorageContainer>
		_containers = new();

	private readonly ConcurrentDictionary<string, IStorageDrive> _drives =
		new(StringComparer.OrdinalIgnoreCase);

	private readonly FileSystemMock _fileSystem;

	public InMemoryStorage(FileSystemMock fileSystem)
	{
		_fileSystem = fileSystem;
		DriveInfoMock mainDrive = DriveInfoMock.New("".PrefixRoot(), _fileSystem);
		_drives.TryAdd(mainDrive.Name, mainDrive);
	}

	#region IStorage Members

	/// <inheritdoc cref="IStorage.CurrentDirectory" />
	public string CurrentDirectory { get; set; } = string.Empty.PrefixRoot();

	/// <inheritdoc cref="IStorage.Copy(IStorageLocation, IStorageLocation, bool)" />
	public IStorageLocation? Copy(IStorageLocation source,
	                              IStorageLocation destination,
	                              bool overwrite = false)
	{
		if (!_containers.TryGetValue(source,
			out IStorageContainer? sourceContainer))
		{
			return null;
		}

		if (sourceContainer.Type != ContainerTypes.File)
		{
			throw ExceptionFactory.AccessToPathDenied(source.FullPath);
		}

		using (_ = sourceContainer.RequestAccess(FileAccess.ReadWrite, FileShare.None))
		{
			if (overwrite &&
			    _containers.TryRemove(destination,
				    out IStorageContainer? existingContainer))
			{
				existingContainer.ClearBytes();
			}

			IStorageContainer copiedContainer =
				InMemoryContainer.NewFile(destination, _fileSystem);
			if (_containers.TryAdd(destination, copiedContainer))
			{
				copiedContainer.WriteBytes(sourceContainer.GetBytes());
				copiedContainer.Attributes = sourceContainer.Attributes;
				copiedContainer.LastWriteTime.Set(
					sourceContainer.LastWriteTime.Get(DateTimeKind.Local),
					DateTimeKind.Local);
				copiedContainer.LastAccessTime.Set(
					sourceContainer.LastAccessTime.Get(DateTimeKind.Local),
					DateTimeKind.Local);
				return destination;
			}

			throw ExceptionFactory.CannotCreateFileWhenAlreadyExists();
		}
	}

	/// <inheritdoc cref="IStorage.DeleteContainer(IStorageLocation, bool)" />
	public bool DeleteContainer(IStorageLocation location, bool recursive = false)
	{
		if (!_containers.TryGetValue(location, out IStorageContainer? container))
		{
			return false;
		}

		if (container.Type == ContainerTypes.Directory)
		{
			string start = location.FullPath +
			               _fileSystem.Path.DirectorySeparatorChar;
			if (recursive)
			{
				foreach (IStorageLocation key in _containers.Keys.Where(x
					=> x.FullPath.StartsWith(start)))
				{
					if (_containers.TryRemove(key,
						out IStorageContainer? removedChild))
					{
						removedChild.ClearBytes();
					}
				}
			}
			else if (_containers.Keys.Any(x => x.FullPath.StartsWith(start)))
			{
				throw ExceptionFactory.DirectoryNotEmpty(location.FullPath);
			}
		}

		if (_containers.TryRemove(location, out IStorageContainer? removed))
		{
			removed.ClearBytes();
			return true;
		}

		return false;
	}

	/// <inheritdoc cref="IStorage.EnumerateLocations(IStorageLocation, ContainerTypes, string, EnumerationOptions?)" />
	public IEnumerable<IStorageLocation> EnumerateLocations(
		IStorageLocation location,
		ContainerTypes type,
		string expression = "*",
		EnumerationOptions? enumerationOptions = null)
	{
		ValidateExpression(expression);
		if (!_containers.ContainsKey(location))
		{
			throw ExceptionFactory.DirectoryNotFound(location.FullPath);
		}

		enumerationOptions ??= EnumerationOptionsHelper.Compatible;

		foreach (KeyValuePair<IStorageLocation, IStorageContainer> item in _containers
		   .Where(x => x.Key.FullPath.StartsWith(location.FullPath) &&
		               !x.Key.Equals(location)))
		{
			string? parentPath =
				_fileSystem.Path.GetDirectoryName(
					item.Key.FullPath.TrimEnd(_fileSystem.Path
					   .DirectorySeparatorChar));
			if (!enumerationOptions.RecurseSubdirectories &&
			    parentPath != location.FullPath)
			{
				continue;
			}

			if (!EnumerationOptionsHelper.MatchesPattern(enumerationOptions,
				_fileSystem.Path.GetFileName(item.Key.FullPath), expression))
			{
				continue;
			}

			if (type.HasFlag(item.Value.Type))
			{
				yield return item.Key;
			}
		}
	}

	/// <inheritdoc cref="IStorage.GetContainer(IStorageLocation)" />
	[return: NotNullIfNotNull("location")]
	public IStorageContainer? GetContainer(IStorageLocation? location)
	{
		if (location == null)
		{
			return null;
		}

		if (_containers.TryGetValue(location, out IStorageContainer? container))
		{
			return container;
		}

		return NullContainer.New(_fileSystem);
	}

	/// <inheritdoc cref="IStorage.GetDrive(string?)" />
	public IStorageDrive? GetDrive(string? driveName)
	{
		if (string.IsNullOrEmpty(driveName))
		{
			return null;
		}

		DriveInfoMock drive = DriveInfoMock.New(driveName, _fileSystem);
		if (_drives.TryGetValue(drive.Name, out IStorageDrive? d))
		{
			return d;
		}

		return null;
	}

	/// <inheritdoc cref="IStorage.GetDrives()" />
	public IEnumerable<IStorageDrive> GetDrives()
		=> _drives.Values;

	/// <inheritdoc cref="IStorage.GetLocation(string?, string?)" />
	[return: NotNullIfNotNull("path")]
	public IStorageLocation? GetLocation(string? path, string? friendlyName = null)
	{
		if (path == null)
		{
			return null;
		}

		IStorageDrive? drive = _fileSystem.Storage.GetDrive(
			_fileSystem.Path.GetPathRoot(path));
		if (drive == null &&
		    !_fileSystem.Path.IsPathRooted(path))
		{
			drive = _fileSystem.Storage.GetDrives().First();
		}

		return InMemoryLocation.New(drive, _fileSystem.Path.GetFullPath(path), path);
	}

	/// <inheritdoc cref="IStorage.GetOrAddDrive(string)" />
	public IStorageDrive GetOrAddDrive(string driveName)
	{
		DriveInfoMock drive = DriveInfoMock.New(driveName, _fileSystem);
		return _drives.GetOrAdd(drive.Name, _ => drive);
	}

	/// <inheritdoc
	///     cref="IStorage.GetOrCreateContainer(IStorageLocation, Func{IStorageLocation, FileSystemMock, IStorageContainer})" />
	public IStorageContainer GetOrCreateContainer(
		IStorageLocation location,
		Func<IStorageLocation, FileSystemMock, IStorageContainer> containerGenerator)
	{
		ChangeDescription? fileSystemChange = null;
		IStorageContainer container = _containers.GetOrAdd(location,
			loc =>
			{
				IStorageContainer container =
					containerGenerator.Invoke(loc, _fileSystem);
				if (container.Type == ContainerTypes.Directory)
				{
					CreateParents(_fileSystem, loc);
					IStorageAccessHandle access =
						container.RequestAccess(FileAccess.Write,
							FileShare.ReadWrite);
					fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
						location.FullPath,
						ChangeTypes.DirectoryCreated,
						NotifyFilters.CreationTime);
					access.Dispose();
				}
				else
				{
					IStorageAccessHandle access =
						container.RequestAccess(FileAccess.Write,
							FileShare.ReadWrite);
					fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
						location.FullPath,
						ChangeTypes.FileCreated,
						NotifyFilters.CreationTime);
					access.Dispose();
				}

				return container;
			});
		_fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
		return container;
	}

	/// <inheritdoc cref="IStorage.Move(IStorageLocation, IStorageLocation, bool, bool)" />
	public IStorageLocation? Move(IStorageLocation source,
	                              IStorageLocation destination,
	                              bool overwrite = false,
	                              bool recursive = false)
	{
		List<Rollback> rollbacks = new();
		try
		{
			return MoveInternal(source, destination, overwrite, recursive, rollbacks);
		}
		catch (Exception)
		{
			foreach (Rollback rollback in rollbacks)
			{
				rollback.Execute();
			}

			throw;
		}
	}

	/// <inheritdoc cref="IStorage.Replace(IStorageLocation, IStorageLocation, IStorageLocation?, bool)" />
	public IStorageLocation? Replace(IStorageLocation source,
	                                 IStorageLocation destination,
	                                 IStorageLocation? backup,
	                                 bool ignoreMetadataErrors = false)
	{
		if (!_containers.TryGetValue(source,
			out IStorageContainer? sourceContainer))
		{
			return null;
		}

		if (!_containers.TryGetValue(destination,
			out IStorageContainer? destinationContainer))
		{
			return null;
		}

		if (sourceContainer.Type != ContainerTypes.File ||
		    destinationContainer.Type != ContainerTypes.File)
		{
			throw ExceptionFactory.AccessToPathDenied(source.FullPath);
		}

		using (_ = sourceContainer.RequestAccess(FileAccess.ReadWrite, FileShare.None,
			ignoreMetadataErrors))
		{
			using (_ = destinationContainer.RequestAccess(FileAccess.ReadWrite,
				FileShare.None, ignoreMetadataErrors))
			{
				if (_containers.TryRemove(destination,
					out IStorageContainer? existingDestinationContainer))
				{
					int destinationBytesLength =
						existingDestinationContainer.GetBytes().Length;
					destination.Drive?.ChangeUsedBytes(-1 * destinationBytesLength);
					if (backup != null)
					{
						backup.Drive?.ChangeUsedBytes(destinationBytesLength);
						_containers.TryAdd(backup, existingDestinationContainer);
					}

					if (_containers.TryRemove(source,
						out IStorageContainer? existingSourceContainer))
					{
						int sourceBytesLength = existingSourceContainer.GetBytes().Length;
						source.Drive?.ChangeUsedBytes(-1 * sourceBytesLength);
						destination.Drive?.ChangeUsedBytes(sourceBytesLength);
						_containers.TryAdd(destination, existingSourceContainer);
						return destination;
					}
				}

				throw ExceptionFactory.CannotCreateFileWhenAlreadyExists();
			}
		}
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IStorage.ResolveLinkTarget(IStorageLocation, bool)" />
	public IStorageLocation? ResolveLinkTarget(IStorageLocation location,
	                                           bool returnFinalTarget = false)
	{
		if (_containers.TryGetValue(location,
			    out IStorageContainer? initialContainer) &&
		    initialContainer.LinkTarget != null)
		{
			IStorageLocation nextLocation =
				_fileSystem.Storage.GetLocation(initialContainer.LinkTarget);
			if (_containers.TryGetValue(nextLocation,
				out IStorageContainer? container))
			{
				if (returnFinalTarget)
				{
					nextLocation = ResolveFinalLinkTarget(container, location) ??
					               nextLocation;
				}

				return nextLocation;
			}

			return nextLocation;
		}

		return null;
	}
#endif

	/// <inheritdoc
	///     cref="IStorage.TryAddContainer(IStorageLocation, Func{IStorageLocation, FileSystemMock, IStorageContainer}, out IStorageContainer?)" />
	public bool TryAddContainer(
		IStorageLocation location,
		Func<IStorageLocation, FileSystemMock, IStorageContainer> containerGenerator,
		[NotNullWhen(true)] out IStorageContainer? container)
	{
		ChangeDescription? fileSystemChange = null;

		container = _containers.GetOrAdd(
			location,
			_ =>
			{
				IStorageContainer container =
					containerGenerator(location, _fileSystem);
				IDisposable access =
					container.RequestAccess(FileAccess.Write, FileShare.ReadWrite);
				fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
					location.FullPath,
					ChangeTypes.FileCreated,
					NotifyFilters.CreationTime);
				access.Dispose();
				return container;
			});

		if (fileSystemChange != null)
		{
			_fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
			return true;
		}

		container = null;
		return false;
	}

	#endregion

	private void CreateParents(FileSystemMock fileSystem, IStorageLocation location)
	{
		List<string> parents = new();
		string? parent = fileSystem.Path.GetDirectoryName(
			location.FullPath.TrimEnd(fileSystem.Path.DirectorySeparatorChar,
				fileSystem.Path.AltDirectorySeparatorChar));
		while (!string.IsNullOrEmpty(parent))
		{
			parents.Add(parent);
			parent = fileSystem.Path.GetDirectoryName(parent);
		}

		parents.Reverse();
		TimeAdjustments timeAdjustments =
			TimeAdjustments.LastWriteTime;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			timeAdjustments |= TimeAdjustments.LastAccessTime;
		}

		List<IStorageAccessHandle> requests = new();
		try
		{
			foreach (string? parentPath in parents)
			{
				ChangeDescription? fileSystemChange = null;
				IStorageLocation parentLocation =
					_fileSystem.Storage.GetLocation(parentPath);
				_ = _containers.AddOrUpdate(
					parentLocation,
					loc =>
					{
						IStorageContainer container =
							InMemoryContainer.NewDirectory(loc, _fileSystem);

						requests.Add(container.RequestAccess(FileAccess.Write,
							FileShare.ReadWrite));
						fileSystemChange =
							fileSystem.ChangeHandler.NotifyPendingChange(
								parentPath,
								ChangeTypes.DirectoryCreated,
								NotifyFilters.CreationTime);
						return container;
					},
					(_, f) =>
					{
						f.AdjustTimes(timeAdjustments);
						return f;
					});
				fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
			}
		}
		finally
		{
			foreach (IStorageAccessHandle request in requests)
			{
				request.Dispose();
			}
		}
	}

	private IStorageLocation? MoveInternal(IStorageLocation source,
	                                       IStorageLocation destination,
	                                       bool overwrite,
	                                       bool recursive,
	                                       List<Rollback>? rollbacks = null)
	{
		if (!_containers.TryGetValue(source,
			out IStorageContainer? container))
		{
			return null;
		}

		List<IStorageLocation> children =
			EnumerateLocations(source, ContainerTypes.DirectoryOrFile).ToList();
		if (children.Any() && !recursive)
		{
			throw ExceptionFactory.DirectoryNotEmpty(source.FullPath);
		}

		using (_ = container.RequestAccess(
			FileAccess.Write, FileShare.None))
		{
			if (children.Any() && recursive)
			{
				foreach (IStorageLocation child in children)
				{
					IStorageLocation childDestination = _fileSystem
					   .GetMoveLocation(child, source, destination);
					MoveInternal(child, childDestination, overwrite, recursive,
						rollbacks: rollbacks);
				}
			}

			if (_containers.TryRemove(source, out IStorageContainer? sourceContainer))
			{
				if (overwrite &&
				    _containers.TryRemove(destination,
					    out IStorageContainer? existingContainer))
				{
					existingContainer.ClearBytes();
				}

				if (_containers.TryAdd(destination, sourceContainer))
				{
					int bytesLength = sourceContainer.GetBytes().Length;
					source.Drive?.ChangeUsedBytes(-1 * bytesLength);
					destination.Drive?.ChangeUsedBytes(bytesLength);
					rollbacks?.Add(new Rollback(
						() => MoveInternal(destination, source, true, false)));
					return destination;
				}

				_containers.TryAdd(source, sourceContainer);
				throw ExceptionFactory.CannotCreateFileWhenAlreadyExists();
			}
		}

		return source;
	}

#if FEATURE_FILESYSTEM_LINK
	private IStorageLocation? ResolveFinalLinkTarget(IStorageContainer container,
	                                                 IStorageLocation originalLocation)
	{
		int maxResolveLinks =
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? 63
				: 40;
		IStorageLocation? nextLocation = null;
		for (int i = 1; i < maxResolveLinks; i++)
		{
			if (container.LinkTarget == null)
			{
				break;
			}

			nextLocation = _fileSystem.Storage.GetLocation(container.LinkTarget);
			if (!_containers.TryGetValue(nextLocation,
				out IStorageContainer? nextContainer))
			{
				return nextLocation;
			}

			container = nextContainer;
		}

		if (container.LinkTarget != null)
		{
			throw ExceptionFactory.FileNameCannotBeResolved(
				originalLocation.FullPath);
		}

		return nextLocation;
	}
#endif

	private static void ValidateExpression(string expression)
	{
		if (expression.Contains('\0'))
		{
			throw ExceptionFactory.PathHasIllegalCharacters(expression);
		}
	}

	private sealed class Rollback
	{
		private readonly Action _onRollback;

		public Rollback(Action onRollback)
		{
			_onRollback = onRollback;
		}

		public void Execute()
		{
			_onRollback.Invoke();
		}
	}
}
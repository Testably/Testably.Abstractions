#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
using Microsoft.Win32.SafeHandles;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class FileMock : IFile
{
	private readonly MockFileSystem _fileSystem;

	internal FileMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IFile Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFile.AppendAllLines(string, IEnumerable{string})" />
	public void AppendAllLines(string path, IEnumerable<string> contents)
		=> AppendAllLines(path, contents, Encoding.Default);

	/// <inheritdoc cref="IFile.AppendAllLines(string, IEnumerable{string}, Encoding)" />
	public void AppendAllLines(
		string path,
		IEnumerable<string> contents,
		Encoding encoding)
	{
		_ = contents ?? throw new ArgumentNullException(nameof(contents));
		_ = encoding ?? throw new ArgumentNullException(nameof(encoding));
		AppendAllText(
			path,
			contents.Aggregate(string.Empty, (a, b) => a + b + Environment.NewLine),
			encoding);
	}

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
		CancellationToken cancellationToken = default)
		=> AppendAllLinesAsync(path, contents, Encoding.Default, cancellationToken);

	/// <inheritdoc cref="IFile.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
	public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
		Encoding encoding,
		CancellationToken cancellationToken = default)
	{
		ThrowIfCancelled(cancellationToken);
		AppendAllLines(path, contents, encoding);
		return Task.CompletedTask;
	}
#endif

	/// <inheritdoc cref="IFile.AppendAllText(string, string?)" />
	public void AppendAllText(string path, string? contents)
		=> AppendAllText(path, contents, Encoding.Default);

	/// <inheritdoc cref="IFile.AppendAllText(string, string?, Encoding)" />
	public void AppendAllText(string path, string? contents, Encoding encoding)
	{
		IStorageContainer fileInfo =
			_fileSystem.Storage.GetOrCreateContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)),
				InMemoryContainer.NewFile);
		if (contents != null)
		{
			using (fileInfo.RequestAccess(
				FileAccess.ReadWrite,
				FileStreamFactoryMock.DefaultShare))
			{
				if (fileInfo.GetBytes().Length == 0)
				{
					fileInfo.WriteBytes(encoding.GetPreamble());
				}

				fileInfo.AppendBytes(encoding.GetBytes(contents));
			}
		}
	}

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.AppendAllTextAsync(string, string?, CancellationToken)" />
	public Task AppendAllTextAsync(string path, string? contents,
		CancellationToken cancellationToken = default)
		=> AppendAllTextAsync(path, contents, Encoding.Default, cancellationToken);

	/// <inheritdoc cref="IFile.AppendAllTextAsync(string, string?, Encoding, CancellationToken)" />
	public Task AppendAllTextAsync(string path, string? contents, Encoding encoding,
		CancellationToken cancellationToken = default)
	{
		ThrowIfCancelled(cancellationToken);
		AppendAllText(path, contents, encoding);
		return Task.CompletedTask;
	}
#endif

	/// <inheritdoc cref="IFile.AppendText(string)" />
	public StreamWriter AppendText(string path)
		=> FileSystem.FileInfo
			.New(path.EnsureValidFormat(FileSystem))
			.AppendText();

	/// <inheritdoc cref="IFile.Copy(string, string)" />
	public void Copy(string sourceFileName, string destFileName)
	{
		sourceFileName.EnsureValidFormat(_fileSystem, nameof(sourceFileName));
		destFileName.EnsureValidFormat(_fileSystem, nameof(destFileName));
		try
		{
			_fileSystem.FileInfo
				.New(sourceFileName)
				.CopyTo(destFileName);
		}
		catch (UnauthorizedAccessException)
		{
			Execute.OnNetFramework(()
				=> throw ExceptionFactory.AccessToPathDenied(sourceFileName));

			throw;
		}
	}

	/// <inheritdoc cref="IFile.Copy(string, string, bool)" />
	public void Copy(string sourceFileName, string destFileName, bool overwrite)
		=> Execute.OnNetFramework(
			() =>
			{
				try
				{
					_fileSystem.FileInfo.New(sourceFileName
							.EnsureValidFormat(_fileSystem, nameof(sourceFileName)))
						.CopyTo(destFileName
								.EnsureValidFormat(_fileSystem, nameof(destFileName)),
							overwrite);
				}
				catch (UnauthorizedAccessException)
				{
					throw ExceptionFactory.AccessToPathDenied(sourceFileName);
				}
			},
			() =>
			{
				_fileSystem.FileInfo.New(sourceFileName
						.EnsureValidFormat(_fileSystem, nameof(sourceFileName)))
					.CopyTo(destFileName
						.EnsureValidFormat(_fileSystem, nameof(destFileName)), overwrite);
			});

	/// <inheritdoc cref="IFile.Create(string)" />
	public FileSystemStream Create(string path)
		=> new FileStreamMock(
			_fileSystem,
			path,
			FileMode.Create,
			FileAccess.ReadWrite,
			FileShare.None);

	/// <inheritdoc cref="IFile.Create(string, int)" />
	public FileSystemStream Create(string path, int bufferSize)
		=> new FileStreamMock(
			_fileSystem,
			path,
			FileMode.Create,
			FileAccess.ReadWrite,
			FileShare.None,
			bufferSize);

	/// <inheritdoc cref="IFile.Create(string, int, FileOptions)" />
	public FileSystemStream Create(string path, int bufferSize, FileOptions options)
		=> new FileStreamMock(
			_fileSystem,
			path,
			FileMode.Create,
			FileAccess.ReadWrite,
			FileShare.None,
			bufferSize,
			options);

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFile.CreateSymbolicLink(string, string)" />
	public IFileSystemInfo CreateSymbolicLink(
		string path, string pathToTarget)
	{
		path.EnsureValidFormat(_fileSystem);
		IFileInfo fileSystemInfo =
			_fileSystem.FileInfo.New(path);
		fileSystemInfo.CreateAsSymbolicLink(pathToTarget);
		return fileSystemInfo;
	}
#endif

	/// <inheritdoc cref="IFile.CreateText(string)" />
	public StreamWriter CreateText(string path)
		=> FileSystem.FileInfo
			.New(path.EnsureValidFormat(FileSystem))
			.CreateText();

	/// <inheritdoc cref="IFile.Decrypt(string)" />
	[SupportedOSPlatform("windows")]
	public void Decrypt(string path)
	{
		IStorageContainer container = GetContainerFromPath(path);
		container.Decrypt();
	}

	/// <inheritdoc cref="IFile.Delete(string)" />
	public void Delete(string path)
		=> _fileSystem.Storage.DeleteContainer(
			_fileSystem.Storage.GetLocation(
				path.EnsureValidFormat(FileSystem)));

	/// <inheritdoc cref="IFile.Encrypt(string)" />
	[SupportedOSPlatform("windows")]
	public void Encrypt(string path)
	{
		IStorageContainer container = GetContainerFromPath(path);
		container.Encrypt();
	}

	/// <inheritdoc cref="IFile.Exists(string?)" />
	public bool Exists([NotNullWhen(true)] string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		return FileInfoMock.New(
				_fileSystem.Storage.GetLocation(path),
				_fileSystem)
			.Exists;
	}

	/// <inheritdoc cref="IFile.GetAttributes(string)" />
	public FileAttributes GetAttributes(string path)
	{
		IStorageContainer container = _fileSystem.Storage
			.GetContainer(_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem))
				.ThrowExceptionIfNotFound(_fileSystem));

		return container.Attributes;
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetAttributes(SafeFileHandle)" />
	public FileAttributes GetAttributes(SafeFileHandle fileHandle)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		return container.Attributes;
	}
#endif

	/// <inheritdoc cref="IFile.GetCreationTime(string)" />
	public DateTime GetCreationTime(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
			.CreationTime.Get(DateTimeKind.Local);

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetCreationTime(SafeFileHandle)" />
	public DateTime GetCreationTime(SafeFileHandle fileHandle)
		=> GetContainerFromSafeFileHandle(fileHandle)
			.CreationTime.Get(DateTimeKind.Local);
#endif

	/// <inheritdoc cref="IFile.GetCreationTimeUtc(string)" />
	public DateTime GetCreationTimeUtc(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
			.CreationTime.Get(DateTimeKind.Utc);

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetCreationTimeUtc(SafeFileHandle)" />
	public DateTime GetCreationTimeUtc(SafeFileHandle fileHandle)
		=> GetContainerFromSafeFileHandle(fileHandle)
			.CreationTime.Get(DateTimeKind.Utc);
#endif

	/// <inheritdoc cref="IFile.GetLastAccessTime(string)" />
	public DateTime GetLastAccessTime(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
			.LastAccessTime.Get(DateTimeKind.Local);

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetLastAccessTime(SafeFileHandle)" />
	public DateTime GetLastAccessTime(SafeFileHandle fileHandle)
		=> GetContainerFromSafeFileHandle(fileHandle)
			.LastAccessTime.Get(DateTimeKind.Local);
#endif

	/// <inheritdoc cref="IFile.GetLastAccessTimeUtc(string)" />
	public DateTime GetLastAccessTimeUtc(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
			.LastAccessTime.Get(DateTimeKind.Utc);

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetLastAccessTimeUtc(SafeFileHandle)" />
	public DateTime GetLastAccessTimeUtc(SafeFileHandle fileHandle)
		=> GetContainerFromSafeFileHandle(fileHandle)
			.LastAccessTime.Get(DateTimeKind.Utc);
#endif

	/// <inheritdoc cref="IFile.GetLastWriteTime(string)" />
	public DateTime GetLastWriteTime(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
			.LastWriteTime.Get(DateTimeKind.Local);

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetLastWriteTime(SafeFileHandle)" />
	public DateTime GetLastWriteTime(SafeFileHandle fileHandle)
		=> GetContainerFromSafeFileHandle(fileHandle)
			.LastWriteTime.Get(DateTimeKind.Local);
#endif

	/// <inheritdoc cref="IFile.GetLastWriteTimeUtc(string)" />
	public DateTime GetLastWriteTimeUtc(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
			.LastWriteTime.Get(DateTimeKind.Utc);

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetLastWriteTimeUtc(SafeFileHandle)" />
	public DateTime GetLastWriteTimeUtc(SafeFileHandle fileHandle)
		=> GetContainerFromSafeFileHandle(fileHandle)
			.LastWriteTime.Get(DateTimeKind.Utc);
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IFile.GetUnixFileMode(string)" />
	[UnsupportedOSPlatform("windows")]
	public UnixFileMode GetUnixFileMode(string path)
		=> Execute.OnWindows(
			() => throw ExceptionFactory.UnixFileModeNotSupportedOnThisPlatform(),
			() => _fileSystem.Storage.GetContainer(
					_fileSystem.Storage.GetLocation(
							path.EnsureValidFormat(FileSystem))
						.ThrowExceptionIfNotFound(_fileSystem))
				.UnixFileMode);
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.GetUnixFileMode(SafeFileHandle)" />
	[UnsupportedOSPlatform("windows")]
	public UnixFileMode GetUnixFileMode(SafeFileHandle fileHandle)
		=> Execute.OnWindows(
			() => throw ExceptionFactory.UnixFileModeNotSupportedOnThisPlatform(),
			() => GetContainerFromSafeFileHandle(fileHandle)
				.UnixFileMode);
#endif

	/// <inheritdoc cref="IFile.Move(string, string)" />
	public void Move(string sourceFileName, string destFileName)
		=> _fileSystem.FileInfo.New(sourceFileName
				.EnsureValidFormat(_fileSystem, nameof(sourceFileName)))
			.MoveTo(destFileName
				.EnsureValidFormat(_fileSystem, nameof(destFileName)));

#if FEATURE_FILE_MOVETO_OVERWRITE
	/// <inheritdoc cref="IFile.Move(string, string, bool)" />
	public void Move(string sourceFileName, string destFileName, bool overwrite)
		=> _fileSystem.FileInfo.New(sourceFileName
				.EnsureValidFormat(_fileSystem, nameof(sourceFileName)))
			.MoveTo(destFileName
				.EnsureValidFormat(_fileSystem, nameof(destFileName)), overwrite);
#endif

	/// <inheritdoc cref="IFile.Open(string, FileMode)" />
	public FileSystemStream Open(string path, FileMode mode)
		=> new FileStreamMock(
			_fileSystem,
			path,
			mode,
			mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite,
			FileShare.None);

	/// <inheritdoc cref="IFile.Open(string, FileMode, FileAccess)" />
	public FileSystemStream Open(string path, FileMode mode, FileAccess access)
		=> new FileStreamMock(
			_fileSystem,
			path,
			mode,
			access,
			FileShare.None);

	/// <inheritdoc cref="IFile.Open(string, FileMode, FileAccess, FileShare)" />
	public FileSystemStream Open(
		string path,
		FileMode mode,
		FileAccess access,
		FileShare share)
		=> new FileStreamMock(
			_fileSystem,
			path,
			mode,
			access,
			share);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	/// <inheritdoc cref="IFile.Open(string, FileStreamOptions)" />
	public FileSystemStream Open(string path, FileStreamOptions options)
		=> new FileStreamMock(
			_fileSystem,
			path,
			options.Mode,
			options.Access,
			options.Share,
			options.BufferSize,
			options.Options);
#endif

	/// <inheritdoc cref="IFile.OpenRead(string)" />
	public FileSystemStream OpenRead(string path)
		=> new FileStreamMock(
			_fileSystem,
			path,
			FileMode.Open,
			FileAccess.Read);

	/// <inheritdoc cref="IFile.OpenText(string)" />
	public StreamReader OpenText(string path)
		=> FileSystem.FileInfo
			.New(path.EnsureValidFormat(FileSystem))
			.OpenText();

	/// <inheritdoc cref="IFile.OpenWrite(string)" />
	public FileSystemStream OpenWrite(string path)
		=> new FileStreamMock(
			_fileSystem,
			path,
			FileMode.OpenOrCreate,
			FileAccess.Write,
			FileShare.None);

	/// <inheritdoc cref="IFile.ReadAllBytes(string)" />
	public byte[] ReadAllBytes(string path)
	{
		IStorageContainer container = GetContainerFromPath(path);
		using (container.RequestAccess(
			FileAccess.Read,
			FileStreamFactoryMock.DefaultShare))
		{
			Execute.NotOnWindows(() => 
				container.AdjustTimes(TimeAdjustments.LastAccessTime));

			return container.GetBytes().ToArray();
		}
	}

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.ReadAllBytesAsync(string, CancellationToken)" />
	public Task<byte[]> ReadAllBytesAsync(string path,
		CancellationToken cancellationToken =
			default)
	{
		ThrowIfCancelled(cancellationToken);
		return Task.FromResult(ReadAllBytes(path));
	}
#endif

	/// <inheritdoc cref="IFile.ReadAllLines(string)" />
	public string[] ReadAllLines(string path)
		=> ReadAllLines(path, Encoding.Default);

	/// <inheritdoc cref="IFile.ReadAllLines(string, Encoding)" />
	public string[] ReadAllLines(string path, Encoding encoding)
		=> ReadLines(path, encoding).ToArray();

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.ReadAllLinesAsync(string, CancellationToken)" />
	public Task<string[]> ReadAllLinesAsync(
		string path,
		CancellationToken cancellationToken = default)
		=> ReadAllLinesAsync(path, Encoding.Default, cancellationToken);

	/// <inheritdoc cref="IFile.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
	public Task<string[]> ReadAllLinesAsync(
		string path,
		Encoding encoding,
		CancellationToken cancellationToken = default)
	{
		ThrowIfCancelled(cancellationToken);
		return Task.FromResult(ReadAllLines(path, encoding));
	}
#endif

	/// <inheritdoc cref="IFile.ReadAllText(string)" />
	public string ReadAllText(string path)
		=> ReadAllText(path, Encoding.Default);

	/// <inheritdoc cref="IFile.ReadAllText(string, Encoding)" />
	public string ReadAllText(string path, Encoding encoding)
	{
		IStorageContainer container = GetContainerFromPath(path);
		using (container.RequestAccess(
			FileAccess.Read,
			FileStreamFactoryMock.DefaultShare))
		{
			Execute.NotOnWindows(() =>
				container.AdjustTimes(TimeAdjustments.LastAccessTime));

			using (MemoryStream ms = new(container.GetBytes()))
			using (StreamReader sr = new(ms, encoding))
			{
				return sr.ReadToEnd();
			}
		}
	}

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.ReadAllTextAsync(string, CancellationToken)" />
	public Task<string> ReadAllTextAsync(
		string path,
		CancellationToken cancellationToken = default)
		=> ReadAllTextAsync(path, Encoding.Default, cancellationToken);

	/// <inheritdoc cref="IFile.ReadAllTextAsync(string, Encoding, CancellationToken)" />
	public Task<string> ReadAllTextAsync(
		string path,
		Encoding encoding,
		CancellationToken cancellationToken = default)
	{
		ThrowIfCancelled(cancellationToken);
		return Task.FromResult(ReadAllText(path, encoding));
	}
#endif

	/// <inheritdoc cref="IFile.ReadLines(string)" />
	public IEnumerable<string> ReadLines(string path)
		=> ReadLines(path, Encoding.Default);

	/// <inheritdoc cref="IFile.ReadLines(string, Encoding)" />
	public IEnumerable<string> ReadLines(string path, Encoding encoding)
		=> EnumerateLines(ReadAllText(path, encoding));

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IFile.ReadLinesAsync(string, CancellationToken)" />
	public IAsyncEnumerable<string> ReadLinesAsync(string path,
		CancellationToken cancellationToken =
			default)
	{
		ThrowIfCancelled(cancellationToken);
		return ReadAllLines(path).ToAsyncEnumerable();
	}

	/// <inheritdoc cref="IFile.ReadLinesAsync(string, Encoding, CancellationToken)" />
	public IAsyncEnumerable<string> ReadLinesAsync(string path, Encoding encoding,
		CancellationToken cancellationToken =
			default)
	{
		ThrowIfCancelled(cancellationToken);
		return ReadAllLines(path, encoding).ToAsyncEnumerable();
	}
#endif

	/// <inheritdoc cref="IFile.Replace(string, string, string)" />
	public void Replace(string sourceFileName,
		string destinationFileName,
		string? destinationBackupFileName)
		=> _fileSystem.FileInfo.New(sourceFileName
				.EnsureValidFormat(_fileSystem, nameof(sourceFileName)))
			.Replace(destinationFileName
					.EnsureValidFormat(_fileSystem, nameof(destinationFileName)),
				destinationBackupFileName);

	/// <inheritdoc cref="IFile.Replace(string, string, string, bool)" />
	public void Replace(string sourceFileName,
		string destinationFileName,
		string? destinationBackupFileName,
		bool ignoreMetadataErrors)
		=> _fileSystem.FileInfo.New(sourceFileName
				.EnsureValidFormat(_fileSystem, nameof(sourceFileName)))
			.Replace(destinationFileName
					.EnsureValidFormat(_fileSystem, nameof(destinationFileName)),
				destinationBackupFileName,
				ignoreMetadataErrors);

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFile.ResolveLinkTarget(string, bool)" />
	public IFileSystemInfo? ResolveLinkTarget(
		string linkPath, bool returnFinalTarget)
	{
		IStorageLocation location =
			_fileSystem.Storage.GetLocation(linkPath
				.EnsureValidFormat(_fileSystem, nameof(linkPath)));
		Execute.OnWindows(
			() => location.ThrowExceptionIfNotFound(_fileSystem),
			() => location.ThrowExceptionIfNotFound(_fileSystem,
				onDirectoryNotFound: ExceptionFactory.FileNotFound));
		try
		{
			IStorageLocation? targetLocation =
				_fileSystem.Storage.ResolveLinkTarget(location,
					returnFinalTarget);
			if (targetLocation != null)
			{
				return FileSystemInfoMock.New(targetLocation, _fileSystem);
			}

			return null;
		}
		catch (IOException)
		{
			throw ExceptionFactory.FileNameCannotBeResolved(linkPath,
				Execute.IsWindows ? -2147022975 : -2146232800);
		}
	}
#endif

	/// <inheritdoc cref="IFile.SetAttributes(string, FileAttributes)" />
	public void SetAttributes(string path, FileAttributes fileAttributes)
	{
		IStorageContainer container = GetContainerFromPath(path);
		container.Attributes = fileAttributes;
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetAttributes(SafeFileHandle, FileAttributes)" />
	public void SetAttributes(SafeFileHandle fileHandle, FileAttributes fileAttributes)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.Attributes = fileAttributes;
	}
#endif

	/// <inheritdoc cref="IFile.SetCreationTime(string, DateTime)" />
	public void SetCreationTime(string path, DateTime creationTime)
	{
		IStorageContainer container =
			GetContainerFromPath(path, ExceptionMode.FileNotFoundExceptionOnLinuxAndMac);
		container.CreationTime.Set(creationTime, DateTimeKind.Local);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetCreationTime(SafeFileHandle, DateTime)" />
	public void SetCreationTime(SafeFileHandle fileHandle, DateTime creationTime)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.CreationTime.Set(creationTime, DateTimeKind.Local);
	}
#endif

	/// <inheritdoc cref="IFile.SetCreationTimeUtc(string, DateTime)" />
	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		IStorageContainer container =
			GetContainerFromPath(path, ExceptionMode.FileNotFoundExceptionOnLinuxAndMac);
		container.CreationTime.Set(creationTimeUtc, DateTimeKind.Utc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetCreationTimeUtc(SafeFileHandle, DateTime)" />
	public void SetCreationTimeUtc(SafeFileHandle fileHandle, DateTime creationTimeUtc)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.CreationTime.Set(creationTimeUtc, DateTimeKind.Utc);
	}
#endif

	/// <inheritdoc cref="IFile.SetLastAccessTime(string, DateTime)" />
	public void SetLastAccessTime(string path, DateTime lastAccessTime)
	{
		IStorageContainer container =
			GetContainerFromPath(path, ExceptionMode.FileNotFoundExceptionOnLinuxAndMac);
		container.LastAccessTime.Set(lastAccessTime, DateTimeKind.Local);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetLastAccessTime(SafeFileHandle, DateTime)" />
	public void SetLastAccessTime(SafeFileHandle fileHandle, DateTime lastAccessTime)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.LastAccessTime.Set(lastAccessTime, DateTimeKind.Local);
	}
#endif

	/// <inheritdoc cref="IFile.SetLastAccessTimeUtc(string, DateTime)" />
	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		IStorageContainer container =
			GetContainerFromPath(path, ExceptionMode.FileNotFoundExceptionOnLinuxAndMac);
		container.LastAccessTime.Set(lastAccessTimeUtc, DateTimeKind.Utc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetLastAccessTimeUtc(SafeFileHandle, DateTime)" />
	public void SetLastAccessTimeUtc(SafeFileHandle fileHandle,
		DateTime lastAccessTimeUtc)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.LastAccessTime.Set(lastAccessTimeUtc, DateTimeKind.Utc);
	}
#endif

	/// <inheritdoc cref="IFile.SetLastWriteTime(string, DateTime)" />
	public void SetLastWriteTime(string path, DateTime lastWriteTime)
	{
		IStorageContainer container =
			GetContainerFromPath(path, ExceptionMode.FileNotFoundExceptionOnLinuxAndMac);
		container.LastWriteTime.Set(lastWriteTime, DateTimeKind.Local);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetLastWriteTime(SafeFileHandle, DateTime)" />
	public void SetLastWriteTime(SafeFileHandle fileHandle, DateTime lastWriteTime)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.LastWriteTime.Set(lastWriteTime, DateTimeKind.Local);
	}
#endif

	/// <inheritdoc cref="IFile.SetLastWriteTimeUtc(string, DateTime)" />
	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		IStorageContainer container =
			GetContainerFromPath(path, ExceptionMode.FileNotFoundExceptionOnLinuxAndMac);
		container.LastWriteTime.Set(lastWriteTimeUtc, DateTimeKind.Utc);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetLastWriteTimeUtc(SafeFileHandle, DateTime)" />
	public void SetLastWriteTimeUtc(SafeFileHandle fileHandle, DateTime lastWriteTimeUtc)
	{
		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.LastWriteTime.Set(lastWriteTimeUtc, DateTimeKind.Utc);
	}
#endif

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IFile.SetUnixFileMode(string, UnixFileMode)" />
	[UnsupportedOSPlatform("windows")]
	public void SetUnixFileMode(string path, UnixFileMode mode)
	{
		Execute.OnWindows(
			() => throw ExceptionFactory.UnixFileModeNotSupportedOnThisPlatform());

		IStorageContainer container = GetContainerFromPath(path);
		container.UnixFileMode = mode;
	}
#endif

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	/// <inheritdoc cref="IFile.SetUnixFileMode(SafeFileHandle, UnixFileMode)" />
	[UnsupportedOSPlatform("windows")]
	public void SetUnixFileMode(SafeFileHandle fileHandle, UnixFileMode mode)
	{
		Execute.OnWindows(
			() => throw ExceptionFactory.UnixFileModeNotSupportedOnThisPlatform());

		IStorageContainer container = GetContainerFromSafeFileHandle(fileHandle);
		container.UnixFileMode = mode;
	}
#endif

	/// <inheritdoc cref="IFile.WriteAllBytes(string, byte[])" />
	public void WriteAllBytes(string path, byte[] bytes)
	{
		_ = bytes ?? throw new ArgumentNullException(nameof(bytes));
		IStorageContainer container =
			_fileSystem.Storage.GetOrCreateContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)),
				InMemoryContainer.NewFile);
		if (container is not NullContainer)
		{
			Execute.OnWindowsIf(
				container.Attributes.HasFlag(FileAttributes.Hidden),
				() => throw ExceptionFactory.AccessToPathDenied());
			using (container.RequestAccess(
				FileAccess.Write,
				FileStreamFactoryMock.DefaultShare))
			{
				container.WriteBytes(bytes);
			}
		}
	}

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.WriteAllBytesAsync(string, byte[], CancellationToken)" />
	public Task WriteAllBytesAsync(string path, byte[] bytes,
		CancellationToken cancellationToken = default)
	{
		ThrowIfCancelled(cancellationToken);
		WriteAllBytes(path, bytes);
		return Task.CompletedTask;
	}
#endif

	/// <inheritdoc cref="IFile.WriteAllLines(string, string[])" />
	public void WriteAllLines(string path, string[] contents)
		=> WriteAllLines(path, contents, Encoding.Default);

	/// <inheritdoc cref="IFile.WriteAllLines(string, IEnumerable{string})" />
	public void WriteAllLines(string path, IEnumerable<string> contents)
		=> WriteAllLines(path, contents, Encoding.Default);

	/// <inheritdoc cref="IFile.WriteAllLines(string, string[], Encoding)" />
	public void WriteAllLines(
		string path,
		string[] contents,
		Encoding encoding)
		=> WriteAllLines(path, contents.AsEnumerable(), encoding);

	/// <inheritdoc cref="IFile.WriteAllLines(string, IEnumerable{string}, Encoding)" />
	public void WriteAllLines(
		string path,
		IEnumerable<string> contents,
		Encoding encoding)
		=> WriteAllText(
			path,
			contents.Aggregate(string.Empty, (a, b) => a + b + Environment.NewLine),
			encoding);

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
	public Task WriteAllLinesAsync(
		string path,
		IEnumerable<string> contents,
		CancellationToken cancellationToken = default)
		=> WriteAllLinesAsync(path, contents, Encoding.Default, cancellationToken);

	/// <inheritdoc cref="IFile.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
	public Task WriteAllLinesAsync(
		string path,
		IEnumerable<string> contents,
		Encoding encoding,
		CancellationToken cancellationToken = default)
	{
		ThrowIfCancelled(cancellationToken);
		WriteAllLines(path, contents, encoding);
		return Task.CompletedTask;
	}
#endif

	/// <inheritdoc cref="IFile.WriteAllText(string, string?)" />
	public void WriteAllText(string path, string? contents)
		=> WriteAllText(path, contents, Encoding.Default);

	/// <inheritdoc cref="IFile.WriteAllText(string, string?, Encoding)" />
	public void WriteAllText(string path, string? contents, Encoding encoding)
	{
		IStorageContainer container =
			_fileSystem.Storage.GetOrCreateContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)),
				InMemoryContainer.NewFile);
		if (container is not NullContainer && contents != null)
		{
			Execute.OnWindowsIf(
				container.Attributes.HasFlag(FileAttributes.Hidden),
				() => throw ExceptionFactory.AccessToPathDenied());
			using (container.RequestAccess(
				FileAccess.Write,
				FileStreamFactoryMock.DefaultShare))
			{
				container.WriteBytes(encoding.GetPreamble());
				container.AppendBytes(encoding.GetBytes(contents));
			}
		}
	}

#if FEATURE_FILESYSTEM_ASYNC
	/// <inheritdoc cref="IFile.WriteAllTextAsync(string, string?, CancellationToken)" />
	public Task WriteAllTextAsync(string path, string? contents,
		CancellationToken cancellationToken = default)
		=> WriteAllTextAsync(path, contents, Encoding.Default, cancellationToken);

	/// <inheritdoc cref="IFile.WriteAllTextAsync(string, string?, Encoding, CancellationToken)" />
	public Task WriteAllTextAsync(string path, string? contents, Encoding encoding,
		CancellationToken cancellationToken = default)
	{
		ThrowIfCancelled(cancellationToken);
		WriteAllText(path, contents, encoding);
		return Task.CompletedTask;
	}
#endif

	#endregion

	private static IEnumerable<string> EnumerateLines(string contents)
	{
		using (StringReader reader = new(contents))
		{
			while (reader.ReadLine() is { } line)
			{
				yield return line;
			}
		}
	}

	private enum ExceptionMode
	{
		Default,
		FileNotFoundExceptionOnLinuxAndMac
	}

	private IStorageContainer GetContainerFromPath(string path,
		ExceptionMode exceptionMode = ExceptionMode.Default)
	{
		path.EnsureValidFormat(FileSystem);
		IStorageLocation location = _fileSystem.Storage.GetLocation(path);
		if (exceptionMode == ExceptionMode.FileNotFoundExceptionOnLinuxAndMac)
		{
			Execute.OnWindows(
				() => location.ThrowExceptionIfNotFound(_fileSystem),
				() => location.ThrowExceptionIfNotFound(_fileSystem,
					onDirectoryNotFound: ExceptionFactory.FileNotFound));
		}

		if (exceptionMode == ExceptionMode.Default)
		{
			location.ThrowExceptionIfNotFound(_fileSystem);
		}

		return _fileSystem.Storage.GetContainer(location);
	}

#if FEATURE_FILESYSTEM_SAFEFILEHANDLE
	private IStorageContainer GetContainerFromSafeFileHandle(SafeFileHandle fileHandle)
	{
		SafeFileHandleMock safeFileHandleMock = _fileSystem
			.SafeFileHandleStrategy.MapSafeFileHandle(fileHandle);
		IStorageContainer container = _fileSystem.Storage
			.GetContainer(_fileSystem.Storage.GetLocation(
					safeFileHandleMock.Path)
				.ThrowExceptionIfNotFound(_fileSystem));
		if (container is NullContainer)
		{
			throw ExceptionFactory.FileNotFound("");
		}

		return container;
	}
#endif

#if FEATURE_FILESYSTEM_ASYNC
	private static void ThrowIfCancelled(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			throw ExceptionFactory.TaskWasCanceled();
		}
	}
#endif
}

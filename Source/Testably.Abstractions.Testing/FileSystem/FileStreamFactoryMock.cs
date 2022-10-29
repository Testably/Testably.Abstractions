﻿using System.IO;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class FileStreamFactoryMock : IFileStreamFactory
{
	internal const FileShare DefaultShare = FileShare.Read;
	private const int DefaultBufferSize = 4096;
	private const bool DefaultUseAsync = false;
	private readonly MockFileSystem _fileSystem;

	internal FileStreamFactoryMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IFileStreamFactory Members

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode)" />
	public FileSystemStream New(string path, FileMode mode)
		=> New(path,
			mode,
			mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite,
			DefaultShare,
			DefaultBufferSize,
			DefaultUseAsync);

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess)" />
	public FileSystemStream New(string path, FileMode mode, FileAccess access)
		=> New(path, mode, access, DefaultShare, DefaultBufferSize, DefaultUseAsync);

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare)" />
	public FileSystemStream New(string path,
								FileMode mode,
								FileAccess access,
								FileShare share)
		=> New(path, mode, access, share, DefaultBufferSize, DefaultUseAsync);

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int)" />
	public FileSystemStream New(string path,
								FileMode mode,
								FileAccess access,
								FileShare share,
								int bufferSize)
		=> New(path, mode, access, share, bufferSize, DefaultUseAsync);

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, bool)" />
	public FileSystemStream New(string path,
								FileMode mode,
								FileAccess access,
								FileShare share,
								int bufferSize,
								bool useAsync)
		=> New(path,
			mode,
			access,
			share,
			bufferSize,
			useAsync ? FileOptions.Asynchronous : FileOptions.None);

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, FileOptions)" />
	public FileSystemStream New(string path,
								FileMode mode,
								FileAccess access,
								FileShare share,
								int bufferSize,
								FileOptions options)
		=> new FileStreamMock(_fileSystem,
			path,
			mode,
			access,
			share,
			bufferSize,
			options);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	/// <inheritdoc cref="IFileStreamFactory.New(string, FileStreamOptions)" />
	public FileSystemStream New(string path, FileStreamOptions options)
		=> New(path,
			options.Mode,
			options.Access,
			options.Share,
			options.BufferSize,
			options.Options);
#endif

	/// <inheritdoc cref="IFileStreamFactory.Wrap(FileStream)" />
	public FileSystemStream Wrap(FileStream fileStream)
		=> throw ExceptionFactory.NotSupportedFileStreamWrapping();

	#endregion
}
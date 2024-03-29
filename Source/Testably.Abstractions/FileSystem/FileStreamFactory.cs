﻿using Microsoft.Win32.SafeHandles;
using System.IO;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.FileSystem;

internal sealed class FileStreamFactory : IFileStreamFactory
{
	internal FileStreamFactory(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IFileStreamFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode)" />
	public FileSystemStream New(string path, FileMode mode)
		=> Wrap(new FileStream(path, mode));

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess)" />
	public FileSystemStream New(string path, FileMode mode, FileAccess access)
		=> Wrap(new FileStream(path, mode, access));

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare)" />
	public FileSystemStream New(string path,
		FileMode mode,
		FileAccess access,
		FileShare share)
		=> Wrap(new FileStream(path, mode, access, share));

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int)" />
	public FileSystemStream New(string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		int bufferSize)
		=> Wrap(new FileStream(path, mode, access, share, bufferSize));

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, bool)" />
	public FileSystemStream New(string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		int bufferSize,
		bool useAsync)
		=> Wrap(new FileStream(path, mode, access, share, bufferSize, useAsync));

	/// <inheritdoc cref="IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, FileOptions)" />
	public FileSystemStream New(string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		int bufferSize,
		FileOptions options)
		=> Wrap(new FileStream(path, mode, access, share, bufferSize, options));

	/// <inheritdoc cref="IFileStreamFactory.New(SafeFileHandle, FileAccess)" />
#if NET6_0_OR_GREATER
	[ExcludeFromCodeCoverage(Justification = "SafeFileHandle cannot be unit tested.")]
#endif
	public FileSystemStream New(SafeFileHandle handle, FileAccess access)
		=> Wrap(new FileStream(handle, access));

	/// <inheritdoc cref="IFileStreamFactory.New(SafeFileHandle, FileAccess, int)" />
#if NET6_0_OR_GREATER
	[ExcludeFromCodeCoverage(Justification = "SafeFileHandle cannot be unit tested.")]
#endif
	public FileSystemStream New(SafeFileHandle handle, FileAccess access, int bufferSize)
		=> Wrap(new FileStream(handle, access, bufferSize));

	/// <inheritdoc cref="IFileStreamFactory.New(SafeFileHandle, FileAccess, int, bool)" />
#if NET6_0_OR_GREATER
	[ExcludeFromCodeCoverage(Justification = "SafeFileHandle cannot be unit tested.")]
#endif
	public FileSystemStream New(SafeFileHandle handle, FileAccess access, int bufferSize,
		bool isAsync)
		=> Wrap(new FileStream(handle, access, bufferSize, isAsync));

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	/// <inheritdoc cref="IFileStreamFactory.New(string, FileStreamOptions)" />
	public FileSystemStream New(string path, FileStreamOptions options)
		=> Wrap(new FileStream(path, options));
#endif

	/// <inheritdoc cref="IFileStreamFactory.Wrap(FileStream)" />
	public FileSystemStream Wrap(FileStream fileStream)
		=> new FileStreamWrapper(fileStream);

	#endregion
}

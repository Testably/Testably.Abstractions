using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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

	/// <inheritdoc cref="IFileStreamFactory.Create(string, FileMode)" />
	[Obsolete("Use `IFileStreamFactory.New(string, FileMode)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(string path, FileMode mode)
		=> New(path, mode);

	/// <inheritdoc cref="IFileStreamFactory.Create(string, FileMode, FileAccess)" />
	[Obsolete("Use `IFileStreamFactory.New(string, FileMode, FileAccess)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(string path, FileMode mode, FileAccess access)
		=> New(path, mode, access);

	/// <inheritdoc cref="IFileStreamFactory.Create(string, FileMode, FileAccess, FileShare)" />
	[Obsolete("Use `IFileStreamFactory.New(string, FileMode, FileAccess, FileShare)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(string path, FileMode mode, FileAccess access, FileShare share)
		=> New(path, mode, access, share);

	/// <inheritdoc cref="IFileStreamFactory.Create(string, FileMode, FileAccess, FileShare, int)" />
	[Obsolete("Use `IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(
		string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		int bufferSize)
		=> New(path, mode, access, share, bufferSize);

	/// <inheritdoc cref="IFileStreamFactory.Create(string, FileMode, FileAccess, FileShare, int, FileOptions)" />
	[Obsolete(
		"Use `IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, FileOptions)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(
		string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		int bufferSize,
		FileOptions options)
		=> New(path, mode, access, share, bufferSize, options);

	/// <inheritdoc cref="IFileStreamFactory.Create(string, FileMode, FileAccess, FileShare, int, bool)" />
	[Obsolete(
		"Use `IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, bool)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(
		string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		int bufferSize,
		bool useAsync)
		=> New(path, mode, access, share, bufferSize, useAsync);

	/// <inheritdoc cref="IFileStreamFactory.Create(SafeFileHandle, FileAccess)" />
	[Obsolete("Use `IFileStreamFactory.New(SafeFileHandle, FileAccess)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(SafeFileHandle handle, FileAccess access)
		=> New(handle, access);

	/// <inheritdoc cref="IFileStreamFactory.Create(SafeFileHandle, FileAccess, int)" />
	[Obsolete("Use `IFileStreamFactory.New(SafeFileHandle, FileAccess, int)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(SafeFileHandle handle, FileAccess access, int bufferSize)
		=> New(handle, access, bufferSize);

	/// <inheritdoc cref="IFileStreamFactory.Create(SafeFileHandle, FileAccess, int, bool)" />
	[Obsolete("Use `IFileStreamFactory.New(SafeFileHandle, FileAccess, int, bool)` instead")]
	[ExcludeFromCodeCoverage]
	public Stream Create(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
		=> New(handle, access, bufferSize, isAsync);

	/// <inheritdoc cref="IFileStreamFactory.Create(IntPtr, FileAccess)" />
	[Obsolete(
		"This method has been deprecated. Please use New(SafeFileHandle, FileAccess) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	[ExcludeFromCodeCoverage]
	public Stream Create(IntPtr handle, FileAccess access)
		=> throw new NotImplementedException();

	/// <inheritdoc cref="IFileStreamFactory.Create(IntPtr, FileAccess, bool)" />
	[Obsolete(
		"This method has been deprecated. Please use New(SafeFileHandle, FileAccess) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	[ExcludeFromCodeCoverage]
	public Stream Create(IntPtr handle, FileAccess access, bool ownsHandle)
		=> throw new NotImplementedException();

	/// <inheritdoc cref="IFileStreamFactory.Create(IntPtr, FileAccess, bool, int)" />
	[Obsolete(
		"This method has been deprecated. Please use New(SafeFileHandle, FileAccess, int) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	[ExcludeFromCodeCoverage]
	public Stream Create(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
		=> throw new NotImplementedException();

	/// <inheritdoc cref="IFileStreamFactory.Create(IntPtr, FileAccess, bool, int, bool)" />
	[Obsolete(
		"This method has been deprecated. Please use New(SafeFileHandle, FileAccess, int, bool) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	[ExcludeFromCodeCoverage]
	public Stream Create(
		IntPtr handle,
		FileAccess access,
		bool ownsHandle,
		int bufferSize,
		bool isAsync)
		=> throw new NotImplementedException();

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

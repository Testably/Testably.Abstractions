using System;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Statistics;

internal sealed class FileSystemStatistics : IFileSystemStatistics, IStatisticsGate
{
	private static readonly AsyncLocal<bool> IsDisabled = new();
	private static readonly AsyncLocal<bool> IsInit = new();

	/// <summary>
	///     The total count of registered statistic calls.
	/// </summary>
	public int TotalCount => _counter;

	internal readonly CallStatistics<IDirectory> Directory;
	internal readonly PathStatistics<IDirectoryInfoFactory, IDirectoryInfo> DirectoryInfo;
	internal readonly PathStatistics<IDriveInfoFactory, IDriveInfo> DriveInfo;
	internal readonly CallStatistics<IFile> File;
	internal readonly PathStatistics<IFileInfoFactory, IFileInfo> FileInfo;
	internal readonly PathStatistics<IFileStreamFactory, FileSystemStream> FileStream;

	internal readonly PathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher>
		FileSystemWatcher;

	internal readonly CallStatistics<IPath> Path;
	private int _counter;

	public FileSystemStatistics(MockFileSystem fileSystem)
	{
		Directory = new CallStatistics<IDirectory>(this, nameof(IFileSystem.Directory));
		DirectoryInfo = new PathStatistics<IDirectoryInfoFactory, IDirectoryInfo>(
			this, fileSystem, nameof(IFileSystem.DirectoryInfo));
		DriveInfo = new PathStatistics<IDriveInfoFactory, IDriveInfo>(
			this, fileSystem, nameof(IFileSystem.DriveInfo));
		File = new CallStatistics<IFile>(this, nameof(IFileSystem.File));
		FileInfo = new PathStatistics<IFileInfoFactory, IFileInfo>(
			this, fileSystem, nameof(IFileSystem.FileInfo));
		FileStream = new PathStatistics<IFileStreamFactory, FileSystemStream>(
			this, fileSystem, nameof(IFileSystem.FileStream));
		FileSystemWatcher = new PathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher>(
			this, fileSystem, nameof(IFileSystem.FileSystemWatcher));
		Path = new CallStatistics<IPath>(this, nameof(IFileSystem.Path));
	}

	#region IFileSystemStatistics Members

	/// <inheritdoc cref="IFileSystemStatistics.Directory" />
	IStatistics<IDirectory> IFileSystemStatistics.Directory => Directory;

	/// <inheritdoc cref="IFileSystemStatistics.DirectoryInfo" />
	IPathStatistics<IDirectoryInfoFactory, IDirectoryInfo> IFileSystemStatistics.DirectoryInfo
		=> DirectoryInfo;

	/// <inheritdoc cref="IFileSystemStatistics.DriveInfo" />
	IPathStatistics<IDriveInfoFactory, IDriveInfo> IFileSystemStatistics.DriveInfo => DriveInfo;

	/// <inheritdoc cref="IFileSystemStatistics.File" />
	IStatistics<IFile> IFileSystemStatistics.File => File;

	/// <inheritdoc cref="IFileSystemStatistics.FileInfo" />
	IPathStatistics<IFileInfoFactory, IFileInfo> IFileSystemStatistics.FileInfo => FileInfo;

	/// <inheritdoc cref="IFileSystemStatistics.FileStream" />
	IPathStatistics<IFileStreamFactory, FileSystemStream> IFileSystemStatistics.FileStream
		=> FileStream;

	/// <inheritdoc cref="IFileSystemStatistics.FileSystemWatcher" />
	IPathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher> IFileSystemStatistics.
		FileSystemWatcher => FileSystemWatcher;

	/// <inheritdoc cref="IFileSystemStatistics.Path" />
	IStatistics<IPath> IFileSystemStatistics.Path => Path;

	#endregion

	#region IStatisticsGate Members

	/// <inheritdoc cref="IStatisticsGate.GetCounter()" />
	public int GetCounter()
	{
		return Interlocked.Increment(ref _counter);
	}

	/// <inheritdoc cref="IStatisticsGate.TryGetLock(out IDisposable)" />
	public bool TryGetLock(out IDisposable release)
	{
		if (IsDisabled.Value)
		{
			release = TemporaryDisable.None;
			return false;
		}

		IsDisabled.Value = true;
		release = new TemporaryDisable(() => IsDisabled.Value = false);
		return true;
	}

	#endregion

	/// <summary>
	///     Ignores all registrations until the return value is disposed.
	/// </summary>
	internal IDisposable Ignore()
	{
		if (IsDisabled.Value)
		{
			return TemporaryDisable.None;
		}

		IsDisabled.Value = true;
		IsInit.Value = true;
		return new TemporaryDisable(() =>
		{
			IsDisabled.Value = false;
			IsInit.Value = false;
		});
	}

	internal bool IsInitializing()
		=> IsInit.Value;

	private sealed class TemporaryDisable : IDisposable
	{
		public static IDisposable None { get; } = new NoOpDisposable();

		private readonly Action _onDispose;

		public TemporaryDisable(Action onDispose)
		{
			_onDispose = onDispose;
		}

		#region IDisposable Members

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose() => _onDispose();

		#endregion
	}
}

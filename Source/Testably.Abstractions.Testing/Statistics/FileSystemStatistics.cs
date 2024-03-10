using System;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Statistics;

internal sealed class FileSystemStatistics : IFileSystemStatistics, IStatisticsGate
{
	internal readonly FileSystemEntryStatistics DirectoryInfo;
	internal readonly CallStatistics Directory;
	internal readonly FileSystemEntryStatistics DriveInfo;
	internal readonly FileSystemEntryStatistics FileInfo;
	internal readonly CallStatistics File;
	internal readonly FileSystemEntryStatistics FileStream;
	internal readonly FileSystemEntryStatistics FileSystemWatcher;
	internal readonly CallStatistics Path;
	private int _counter;

	private static readonly AsyncLocal<bool> IsDisabled = new();

	public FileSystemStatistics(MockFileSystem fileSystem)
	{
		DirectoryInfo = new FileSystemEntryStatistics(this, fileSystem);
		DriveInfo = new FileSystemEntryStatistics(this, fileSystem);
		FileInfo = new FileSystemEntryStatistics(this, fileSystem);
		FileStream = new FileSystemEntryStatistics(this, fileSystem);
		FileSystemWatcher = new FileSystemEntryStatistics(this, fileSystem);
		File = new CallStatistics(this);
		Directory = new CallStatistics(this);
		Path = new CallStatistics(this);
	}

	#region IFileSystemStatistics Members

	/// <inheritdoc cref="IFileSystemStatistics.Directory" />
	IStatistics IFileSystemStatistics.Directory => Directory;

	/// <inheritdoc cref="IFileSystemStatistics.DirectoryInfo" />
	IPathStatistics IFileSystemStatistics.DirectoryInfo => DirectoryInfo;

	/// <inheritdoc cref="IFileSystemStatistics.DriveInfo" />
	IPathStatistics IFileSystemStatistics.DriveInfo => DriveInfo;

	/// <inheritdoc cref="IFileSystemStatistics.File" />
	IStatistics IFileSystemStatistics.File => File;

	/// <inheritdoc cref="IFileSystemStatistics.FileInfo" />
	IPathStatistics IFileSystemStatistics.FileInfo => FileInfo;

	/// <inheritdoc cref="IFileSystemStatistics.FileStream" />
	IPathStatistics IFileSystemStatistics.FileStream => FileStream;

	/// <inheritdoc cref="IFileSystemStatistics.FileSystemWatcher" />
	IPathStatistics IFileSystemStatistics.FileSystemWatcher => FileSystemWatcher;

	/// <inheritdoc cref="IFileSystemStatistics.Path" />
	IStatistics IFileSystemStatistics.Path => Path;

	#endregion

	#region IStatisticsGate Members

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

	/// <inheritdoc cref="IStatisticsGate.GetCounter()" />
	public int GetCounter()
	{
		return Interlocked.Increment(ref _counter);
	}

	#endregion

	private sealed class TemporaryDisable : IDisposable
	{
		public static IDisposable None { get; } = new NoOpDisposable();

		private readonly Action _onDispose;

		public TemporaryDisable(Action onDispose)
		{
			_onDispose = onDispose;
		}

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose() => _onDispose();
	}
}

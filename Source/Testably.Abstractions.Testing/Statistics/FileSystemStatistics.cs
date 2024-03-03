using System;
using System.Threading;

namespace Testably.Abstractions.Testing.Statistics;

internal sealed class FileSystemStatistics : IFileSystemStatistics, IStatisticsGate
{
	internal readonly FileSystemEntryStatistics DirectoryInfoStatistic;
	internal readonly CallStatistics DirectoryStatistics;
	internal readonly FileSystemEntryStatistics DriveInfoStatistics;
	internal readonly FileSystemEntryStatistics FileInfoStatistics;
	internal readonly CallStatistics FileStatistics;
	internal readonly FileSystemEntryStatistics FileStreamStatistics;
	internal readonly FileSystemEntryStatistics FileSystemWatcherStatistics;
	internal readonly CallStatistics PathStatistics;

	private static readonly AsyncLocal<bool> IsDisabled = new();

	public FileSystemStatistics(MockFileSystem fileSystem)
	{
		DirectoryInfoStatistic = new FileSystemEntryStatistics(this, fileSystem);
		DriveInfoStatistics = new FileSystemEntryStatistics(this, fileSystem);
		FileInfoStatistics = new FileSystemEntryStatistics(this, fileSystem);
		FileStreamStatistics = new FileSystemEntryStatistics(this, fileSystem);
		FileSystemWatcherStatistics = new FileSystemEntryStatistics(this, fileSystem);
		FileStatistics = new CallStatistics(this);
		DirectoryStatistics = new CallStatistics(this);
		PathStatistics = new CallStatistics(this);
	}

	#region IFileSystemStatistics Members

	/// <inheritdoc />
	public IStatistics Directory => DirectoryStatistics;

	/// <inheritdoc />
	public IFileSystemEntryStatistics DirectoryInfo => DirectoryInfoStatistic;

	/// <inheritdoc />
	public IFileSystemEntryStatistics DriveInfo => DriveInfoStatistics;

	/// <inheritdoc />
	public IStatistics File => FileStatistics;

	/// <inheritdoc />
	public IFileSystemEntryStatistics FileInfo => FileInfoStatistics;

	/// <inheritdoc />
	public IFileSystemEntryStatistics FileStream => FileStreamStatistics;

	/// <inheritdoc />
	public IFileSystemEntryStatistics FileSystemWatcher => FileSystemWatcherStatistics;

	/// <inheritdoc />
	public IStatistics Path => PathStatistics;

	#endregion

	/// <inheritdoc />
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

	private class TemporaryDisable : IDisposable
	{
		public static IDisposable None { get; } = new TemporaryDisable(() => { });

		private readonly Action _onDispose;

		public TemporaryDisable(Action onDispose)
		{
			_onDispose = onDispose;
		}

		/// <inheritdoc />
		public void Dispose() => _onDispose();
	}
}

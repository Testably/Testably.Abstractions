using System;
using System.Threading;

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

	/// <inheritdoc />
	IStatistics IFileSystemStatistics.Directory => Directory;

	/// <inheritdoc />
	IPathStatistics IFileSystemStatistics.DirectoryInfo => DirectoryInfo;

	/// <inheritdoc />
	IPathStatistics IFileSystemStatistics.DriveInfo => DriveInfo;

	/// <inheritdoc />
	IStatistics IFileSystemStatistics.File => File;

	/// <inheritdoc />
	IPathStatistics IFileSystemStatistics.FileInfo => FileInfo;

	/// <inheritdoc />
	IPathStatistics IFileSystemStatistics.FileStream => FileStream;

	/// <inheritdoc />
	IPathStatistics IFileSystemStatistics.FileSystemWatcher => FileSystemWatcher;

	/// <inheritdoc />
	IStatistics IFileSystemStatistics.Path => Path;

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

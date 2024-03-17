using System;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Statistics;

internal sealed class FileSystemStatistics : IFileSystemStatistics, IStatisticsGate
{
	private static readonly AsyncLocal<bool> IsDisabled = new();

	/// <summary>
	///     The total count of registered statistic calls.
	/// </summary>
	public int TotalCount => _counter;

	internal readonly CallStatistics Directory;
	internal readonly PathStatistics DirectoryInfo;
	internal readonly PathStatistics DriveInfo;
	internal readonly CallStatistics File;
	internal readonly PathStatistics FileInfo;
	internal readonly PathStatistics FileStream;
	internal readonly PathStatistics FileSystemWatcher;
	internal readonly CallStatistics Path;
	private int _counter;

	public FileSystemStatistics(MockFileSystem fileSystem)
	{
		DirectoryInfo = new PathStatistics(this, fileSystem, nameof(IFileSystem.DirectoryInfo));
		DriveInfo = new PathStatistics(this, fileSystem, nameof(IFileSystem.DriveInfo));
		FileInfo = new PathStatistics(this, fileSystem, nameof(IFileSystem.FileInfo));
		FileStream = new PathStatistics(this, fileSystem, nameof(IFileSystem.FileStream));
		FileSystemWatcher =
			new PathStatistics(this, fileSystem, nameof(IFileSystem.FileSystemWatcher));
		File = new CallStatistics(this, nameof(IFileSystem.File));
		Directory = new CallStatistics(this, nameof(IFileSystem.Directory));
		Path = new CallStatistics(this, nameof(IFileSystem.Path));
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

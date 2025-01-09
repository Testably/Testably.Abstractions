namespace Testably.Abstractions.Testing.Statistics;

internal sealed class FileSystemStatistics : IFileSystemStatistics
{
	internal readonly CallStatistics<IDirectory> Directory;
	internal readonly PathStatistics<IDirectoryInfoFactory, IDirectoryInfo> DirectoryInfo;
	internal readonly PathStatistics<IDriveInfoFactory, IDriveInfo> DriveInfo;
	internal readonly CallStatistics<IFile> File;
	internal readonly PathStatistics<IFileInfoFactory, IFileInfo> FileInfo;
	internal readonly PathStatistics<IFileStreamFactory, FileSystemStream> FileStream;

	internal readonly PathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher>
		FileSystemWatcher;
	internal readonly PathStatistics<IFileVersionInfoFactory, IFileVersionInfo>
		FileVersionInfo;

	internal readonly CallStatistics<IPath> Path;
	private readonly MockFileSystem _fileSystem;

	public FileSystemStatistics(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
		IStatisticsGate statisticsGate = fileSystem.Registration;

		Directory = new CallStatistics<IDirectory>(
			statisticsGate, nameof(IFileSystem.Directory));
		DirectoryInfo = new PathStatistics<IDirectoryInfoFactory, IDirectoryInfo>(
			statisticsGate, fileSystem, nameof(IFileSystem.DirectoryInfo));
		DriveInfo = new PathStatistics<IDriveInfoFactory, IDriveInfo>(
			statisticsGate, fileSystem, nameof(IFileSystem.DriveInfo));
		File = new CallStatistics<IFile>(
			statisticsGate, nameof(IFileSystem.File));
		FileInfo = new PathStatistics<IFileInfoFactory, IFileInfo>(
			statisticsGate, fileSystem, nameof(IFileSystem.FileInfo));
		FileStream = new PathStatistics<IFileStreamFactory, FileSystemStream>(
			statisticsGate, fileSystem, nameof(IFileSystem.FileStream));
		FileSystemWatcher = new PathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher>(
			statisticsGate, fileSystem, nameof(IFileSystem.FileSystemWatcher));
		FileVersionInfo = new PathStatistics<IFileVersionInfoFactory, IFileVersionInfo>(
			statisticsGate, fileSystem, nameof(IFileSystem.FileVersionInfo));
		Path = new CallStatistics<IPath>(
			statisticsGate, nameof(IFileSystem.Path));
	}

	#region IFileSystemStatistics Members

	/// <inheritdoc cref="IFileSystemStatistics.TotalCount" />
	public int TotalCount
		=> _fileSystem.Registration.TotalCount;

	/// <inheritdoc cref="IFileSystemStatistics.Directory" />
	IStatistics<IDirectory> IFileSystemStatistics.Directory
		=> Directory;

	/// <inheritdoc cref="IFileSystemStatistics.DirectoryInfo" />
	IPathStatistics<IDirectoryInfoFactory, IDirectoryInfo> IFileSystemStatistics.DirectoryInfo
		=> DirectoryInfo;

	/// <inheritdoc cref="IFileSystemStatistics.DriveInfo" />
	IPathStatistics<IDriveInfoFactory, IDriveInfo> IFileSystemStatistics.DriveInfo
		=> DriveInfo;

	/// <inheritdoc cref="IFileSystemStatistics.File" />
	IStatistics<IFile> IFileSystemStatistics.File
		=> File;

	/// <inheritdoc cref="IFileSystemStatistics.FileInfo" />
	IPathStatistics<IFileInfoFactory, IFileInfo> IFileSystemStatistics.FileInfo
		=> FileInfo;

	/// <inheritdoc cref="IFileSystemStatistics.FileStream" />
	IPathStatistics<IFileStreamFactory, FileSystemStream> IFileSystemStatistics.FileStream
		=> FileStream;

	/// <inheritdoc cref="IFileSystemStatistics.FileSystemWatcher" />
	IPathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher>
		IFileSystemStatistics.FileSystemWatcher
		=> FileSystemWatcher;

	/// <inheritdoc cref="IFileSystemStatistics.FileSystemWatcher" />
	IPathStatistics<IFileVersionInfoFactory, IFileVersionInfo>
		IFileSystemStatistics.FileVersionInfo 
		=> FileVersionInfo;

	/// <inheritdoc cref="IFileSystemStatistics.Path" />
	IStatistics<IPath> IFileSystemStatistics.Path
		=> Path;

	#endregion
}

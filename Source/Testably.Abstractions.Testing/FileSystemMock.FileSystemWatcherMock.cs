using System.Collections.Generic;
using System.IO;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	/// <summary>
	///     Mocked instance of a <see cref="IFileSystem.IFileSystemWatcher" />
	/// </summary>
	public sealed class FileSystemWatcherMock : IFileSystem.IFileSystemWatcher
	{
		private readonly FileSystemMock _fileSystem;
		private readonly List<string> _filters = new();
		private string _path = string.Empty;

		private FileSystemWatcherMock(FileSystemMock fileSystem)
		{
			_fileSystem = fileSystem;
		}

		#region IFileSystemWatcher Members

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.EnableRaisingEvents" />
		public bool EnableRaisingEvents
		{
			get;
			set;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem
			=> _fileSystem;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Filter" />
		public string Filter
		{
			get => _filters.Count == 0 ? "*" : _filters[0];
			set
			{
				_filters.Clear();
				_filters.Add(value);
			}
		}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Filters" />
		public ICollection<string> Filters
			=> _filters;
#endif

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.IncludeSubdirectories" />
		public bool IncludeSubdirectories
		{
			get;
			set;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.InternalBufferSize" />
		public int InternalBufferSize
		{
			get;
			set;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.NotifyFilter" />
		public NotifyFilters NotifyFilter
		{
			get;
			set;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Path" />
		public string Path
		{
			get => _path;
			set
			{
				if (!_fileSystem.Directory.Exists(value))
				{
					throw ExceptionFactory.DirectoryNameDoesNotExist(value);
				}
				_path = value;
			}
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Dispose" />
		public void Dispose()
		{
			//Stop listening
		}

#pragma warning disable CS0067 //TODO: Should be used and re-enabled
		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Changed" />
		public event FileSystemEventHandler? Changed;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Created" />
		public event FileSystemEventHandler? Created;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Deleted" />
		public event FileSystemEventHandler? Deleted;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Error" />
		public event ErrorEventHandler? Error;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcher.Renamed" />
		public event RenamedEventHandler? Renamed;
#pragma warning restore CS0067

		#endregion

		internal static FileSystemWatcherMock New(FileSystemMock fileSystem)
		{
			return new FileSystemWatcherMock(fileSystem);
		}
	}
}
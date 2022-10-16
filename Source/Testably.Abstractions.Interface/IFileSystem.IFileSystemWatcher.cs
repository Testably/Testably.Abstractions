using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.IO.FileSystemWatcher" />.
	/// </summary>
	public interface IFileSystemWatcher : IFileSystemExtensionPoint, IDisposable
	{
		/// <inheritdoc cref="FileSystemWatcher.Container" />
		IContainer? Container { get; }

		/// <inheritdoc cref="FileSystemWatcher.EnableRaisingEvents" />
		bool EnableRaisingEvents { get; set; }

		/// <inheritdoc cref="FileSystemWatcher.Filter" />
		string Filter { get; set; }

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
		/// <inheritdoc cref="FileSystemWatcher.Filters" />
		ICollection<string> Filters { get; }
#endif

		/// <inheritdoc cref="FileSystemWatcher.IncludeSubdirectories" />
		bool IncludeSubdirectories { get; set; }

		/// <inheritdoc cref="FileSystemWatcher.InternalBufferSize" />
		int InternalBufferSize { get; set; }

		/// <inheritdoc cref="FileSystemWatcher.NotifyFilter" />
		NotifyFilters NotifyFilter { get; set; }

		/// <inheritdoc cref="FileSystemWatcher.Path" />
		string Path { get; set; }

		/// <inheritdoc cref="FileSystemWatcher.Site" />
		ISite? Site { get; set; }

		/// <inheritdoc cref="FileSystemWatcher.SynchronizingObject" />
		ISynchronizeInvoke? SynchronizingObject { get; set; }

		/// <inheritdoc cref="FileSystemWatcher.Changed" />
		event FileSystemEventHandler? Changed;

		/// <inheritdoc cref="FileSystemWatcher.Created" />
		event FileSystemEventHandler? Created;

		/// <inheritdoc cref="FileSystemWatcher.Deleted" />
		event FileSystemEventHandler? Deleted;

		/// <inheritdoc cref="FileSystemWatcher.Error" />
		event ErrorEventHandler? Error;

		/// <inheritdoc cref="FileSystemWatcher.Renamed" />
		event RenamedEventHandler? Renamed;

		/// <inheritdoc cref="FileSystemWatcher.BeginInit()" />
		void BeginInit();

		/// <inheritdoc cref="FileSystemWatcher.EndInit()" />
		void EndInit();

		/// <inheritdoc cref="FileSystemWatcher.WaitForChanged(WatcherChangeTypes)" />
		IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType);

		/// <inheritdoc cref="FileSystemWatcher.WaitForChanged(WatcherChangeTypes, int)" />
		IWaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout);

		/// <summary>
		///     Abstractions for <see cref="System.IO.WaitForChangedResult" />.
		/// </summary>
		public interface IWaitForChangedResult
		{
			/// <inheritdoc cref="WaitForChangedResult.ChangeType" />
			WatcherChangeTypes ChangeType { get; }

			/// <inheritdoc cref="WaitForChangedResult.Name" />
			string? Name { get; }

			/// <inheritdoc cref="WaitForChangedResult.OldName" />
			string? OldName { get; }

			/// <inheritdoc cref="WaitForChangedResult.TimedOut" />
			bool TimedOut { get; }
		}
	}
}
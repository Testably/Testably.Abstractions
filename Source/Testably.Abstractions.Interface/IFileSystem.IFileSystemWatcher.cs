﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.IO.FileSystemWatcher" />.
	/// </summary>
	public interface IFileSystemWatcher : IFileSystemExtensionPoint, IDisposable
	{
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
	}
}
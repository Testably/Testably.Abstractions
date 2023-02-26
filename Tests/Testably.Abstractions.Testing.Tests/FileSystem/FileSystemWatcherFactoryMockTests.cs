﻿using System.IO;
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

[Collection(nameof(IDirectoryCleaner))]
public sealed class FileSystemWatcherFactoryMockTests : IDisposable
{
	public string BasePath => _directoryCleaner.BasePath;
	public MockFileSystem FileSystem { get; }
	public RealFileSystem RealFileSystem { get; }
	private readonly IDirectoryCleaner _directoryCleaner;

	public FileSystemWatcherFactoryMockTests()
	{
		FileSystem = new MockFileSystem();
		RealFileSystem = new RealFileSystem();
		string currentDirectory = RealFileSystem.Directory.GetCurrentDirectory();
		_directoryCleaner = RealFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
		FileSystem.InitializeIn(currentDirectory);
		FileSystem.Directory.SetCurrentDirectory(currentDirectory);
	}

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[SkippableTheory]
	[AutoData]
	public void Wrap_ShouldUsePropertiesFromFileSystemWatcher(
		string path, bool includeSubdirectories, NotifyFilters notifyFilter,
		int internalBufferSize, bool enableRaisingEvents, string filter)
	{
		FileSystem.Directory.CreateDirectory(path);
		RealFileSystem.Directory.CreateDirectory(path);
		FileSystemWatcher fileSystemWatcher = new();
		fileSystemWatcher.Path = path;
		fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
		fileSystemWatcher.NotifyFilter = notifyFilter;
		fileSystemWatcher.InternalBufferSize = internalBufferSize;
		fileSystemWatcher.EnableRaisingEvents = enableRaisingEvents;
		fileSystemWatcher.Filter = filter;

		IFileSystemWatcher result = FileSystem.FileSystemWatcher.Wrap(fileSystemWatcher);

		result.Path.Should().Be(fileSystemWatcher.Path);
		result.IncludeSubdirectories.Should().Be(fileSystemWatcher.IncludeSubdirectories);
		result.NotifyFilter.Should().Be(fileSystemWatcher.NotifyFilter);
		result.InternalBufferSize.Should().Be(fileSystemWatcher.InternalBufferSize);
		result.EnableRaisingEvents.Should().Be(fileSystemWatcher.EnableRaisingEvents);
		result.Filter.Should().Be(fileSystemWatcher.Filter);
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[SkippableTheory]
	[AutoData]
	public void Wrap_WithFilters_ShouldUsePropertiesFromFileSystemWatcher(
		string[] filters)
	{
		FileSystemWatcher fileSystemWatcher = new(".");
		foreach (string filter in filters)
		{
			fileSystemWatcher.Filters.Add(filter);
		}

		IFileSystemWatcher result = FileSystem.FileSystemWatcher.Wrap(fileSystemWatcher);

		result.Filters.Should().BeEquivalentTo(filters);
	}
#endif
}

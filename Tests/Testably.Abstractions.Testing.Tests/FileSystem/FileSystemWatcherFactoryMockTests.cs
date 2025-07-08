using System.IO;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

[Collection(nameof(IDirectoryCleaner))]
public sealed class FileSystemWatcherFactoryMockTests : IDisposable
{
	#region Test Setup

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

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[Theory]
	[AutoData]
	public async Task Wrap_ShouldUsePropertiesFromFileSystemWatcher(
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

		await That(result.Path).IsEqualTo(fileSystemWatcher.Path);
		await That(result.IncludeSubdirectories).IsEqualTo(fileSystemWatcher.IncludeSubdirectories);
		await That(result.NotifyFilter).IsEqualTo(fileSystemWatcher.NotifyFilter);
		await That(result.InternalBufferSize).IsEqualTo(fileSystemWatcher.InternalBufferSize);
		await That(result.EnableRaisingEvents).IsEqualTo(fileSystemWatcher.EnableRaisingEvents);
		await That(result.Filter).IsEqualTo(fileSystemWatcher.Filter);
	}

#if FEATURE_FILESYSTEMWATCHER_ADVANCED
	[Theory]
	[AutoData]
	public async Task Wrap_WithFilters_ShouldUsePropertiesFromFileSystemWatcher(
		string[] filters)
	{
		FileSystemWatcher fileSystemWatcher = new(".");
		foreach (string filter in filters)
		{
			fileSystemWatcher.Filters.Add(filter);
		}

		IFileSystemWatcher result = FileSystem.FileSystemWatcher.Wrap(fileSystemWatcher);

		await That(result.Filters).IsEqualTo(filters).InAnyOrder();
	}
#endif
}

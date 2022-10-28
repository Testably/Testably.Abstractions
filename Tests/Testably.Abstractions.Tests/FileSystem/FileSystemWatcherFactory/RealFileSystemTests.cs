#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
using System.IO;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcherFactory;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests :
	FileSystemFileSystemWatcherFactoryTests<RealFileSystem>,
	IDisposable
{
	/// <inheritdoc cref="FileSystemFileSystemWatcherFactoryTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly IDirectoryCleaner _directoryCleaner;

	public RealFileSystemTests(ITestOutputHelper testOutputHelper)
		: base(new RealFileSystem(), new RealTimeSystem())
	{
		_directoryCleaner = FileSystem
		   .SetCurrentDirectoryToEmptyTemporaryDirectory(testOutputHelper.WriteLine);
	}

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[SkippableTheory]
	[AutoData]
	public void Wrap_NotNull_ShouldReturnWrappedInstanceWithSameProperties(
		string path,
		string filter,
		bool includeSubdirectories,
		int internalBufferSize,
		NotifyFilters notifyFilter,
		bool enableRaisingEvents)
	{
		MockFileSystem mockFileSystem = new();
		mockFileSystem.Directory.CreateDirectory(path);
		FileSystem.Directory.CreateDirectory(path);
		System.IO.FileSystemWatcher fileSystemWatcher = new(path)
		{
			Filter = filter,
			IncludeSubdirectories = includeSubdirectories,
			InternalBufferSize = internalBufferSize,
			NotifyFilter = notifyFilter,
			EnableRaisingEvents = enableRaisingEvents
		};

		IFileSystemWatcher resultOnRealSystem =
			FileSystem.FileSystemWatcher.Wrap(fileSystemWatcher);

		resultOnRealSystem.Path.Should().Be(fileSystemWatcher.Path);
		resultOnRealSystem.Filter.Should().Be(fileSystemWatcher.Filter);
		resultOnRealSystem.IncludeSubdirectories.Should()
		   .Be(fileSystemWatcher.IncludeSubdirectories);
		resultOnRealSystem.InternalBufferSize.Should()
		   .Be(fileSystemWatcher.InternalBufferSize);
		resultOnRealSystem.NotifyFilter.Should().Be(fileSystemWatcher.NotifyFilter);
		resultOnRealSystem.EnableRaisingEvents.Should()
		   .Be(fileSystemWatcher.EnableRaisingEvents);

		IFileSystemWatcher resultOnMockSystem =
			mockFileSystem.FileSystemWatcher.Wrap(fileSystemWatcher);

		resultOnMockSystem.Path.Should().Be(fileSystemWatcher.Path);
		resultOnMockSystem.Filter.Should().Be(fileSystemWatcher.Filter);
		resultOnMockSystem.IncludeSubdirectories.Should()
		   .Be(fileSystemWatcher.IncludeSubdirectories);
		resultOnMockSystem.InternalBufferSize.Should()
		   .Be(fileSystemWatcher.InternalBufferSize);
		resultOnMockSystem.NotifyFilter.Should().Be(fileSystemWatcher.NotifyFilter);
		resultOnMockSystem.EnableRaisingEvents.Should()
		   .Be(fileSystemWatcher.EnableRaisingEvents);
	}
}

#endif
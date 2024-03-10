using System.IO;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileSystemWatcherFactoryStatisticsTests
{
	[SkippableFact]
	public void New_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.FileSystemWatcher.New();

		sut.Statistics.FileSystemWatcher.ShouldOnlyContain(nameof(IFileSystemWatcherFactory.New));
	}

	[SkippableFact]
	public void New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		sut.FileSystemWatcher.New(path);

		sut.Statistics.FileSystemWatcher.ShouldOnlyContain(nameof(IFileSystemWatcherFactory.New),
			path);
	}

	[SkippableFact]
	public void New_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string filter = "bar";

		sut.FileSystemWatcher.New(path, filter);

		sut.Statistics.FileSystemWatcher.ShouldOnlyContain(nameof(IFileSystemWatcherFactory.New),
			path, filter);
	}

	[SkippableFact]
	public void Wrap_FileSystemWatcher_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		FileSystemWatcher fileSystemWatcher = new();

		sut.FileSystemWatcher.Wrap(fileSystemWatcher);

		sut.Statistics.FileSystemWatcher.ShouldOnlyContain(nameof(IFileSystemWatcherFactory.Wrap),
			fileSystemWatcher);
	}

}

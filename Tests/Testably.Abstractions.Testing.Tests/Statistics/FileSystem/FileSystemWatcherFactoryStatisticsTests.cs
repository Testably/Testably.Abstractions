using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileSystemWatcherFactoryStatisticsTests
{
	[SkippableFact]
	public void Method_New_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		using IFileSystemWatcher result = sut.FileSystemWatcher.New();

		sut.StatisticsRegistration.TotalCount.Should().Be(1);
		sut.Statistics.FileSystemWatcher.ShouldOnlyContainMethodCall(
			nameof(IFileSystemWatcherFactory.New));
	}

	[SkippableFact]
	public void Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		using IFileSystemWatcher result = sut.FileSystemWatcher.New(path);

		sut.StatisticsRegistration.TotalCount.Should().Be(1);
		sut.Statistics.FileSystemWatcher.ShouldOnlyContainMethodCall(
			nameof(IFileSystemWatcherFactory.New),
			path);
	}

	[SkippableFact]
	public void Method_New_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string filter = "bar";

		using IFileSystemWatcher result = sut.FileSystemWatcher.New(path, filter);

		sut.StatisticsRegistration.TotalCount.Should().Be(1);
		sut.Statistics.FileSystemWatcher.ShouldOnlyContainMethodCall(
			nameof(IFileSystemWatcherFactory.New),
			path, filter);
	}

	[SkippableFact]
	public void Method_Wrap_FileSystemWatcher_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize();
		FileSystemWatcher fileSystemWatcher = new();

		using IFileSystemWatcher result = sut.FileSystemWatcher.Wrap(fileSystemWatcher);

		sut.StatisticsRegistration.TotalCount.Should().Be(1);
		sut.Statistics.FileSystemWatcher.ShouldOnlyContainMethodCall(
			nameof(IFileSystemWatcherFactory.Wrap),
			fileSystemWatcher);
	}

	[SkippableFact]
	public void ToString_ShouldBeFileSystemWatcher()
	{
		IPathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher> sut
			= new MockFileSystem().Statistics.FileSystemWatcher;

		string? result = sut.ToString();

		result.Should().Be("FileSystemWatcher");
	}
}

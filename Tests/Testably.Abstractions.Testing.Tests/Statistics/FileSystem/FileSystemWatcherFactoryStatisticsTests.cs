using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class FileSystemWatcherFactoryStatisticsTests
{
	[Fact]
	public async Task Method_New_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		using IFileSystemWatcher result = sut.FileSystemWatcher.New();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileSystemWatcher).OnlyContainsMethodCall(
			nameof(IFileSystemWatcherFactory.New));
	}

	[Fact]
	public async Task Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";

		using IFileSystemWatcher result = sut.FileSystemWatcher.New(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileSystemWatcher).OnlyContainsMethodCall(
			nameof(IFileSystemWatcherFactory.New),
			path);
	}

	[Fact]
	public async Task Method_New_String_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory("foo");
		string path = "foo";
		string filter = "bar";

		using IFileSystemWatcher result = sut.FileSystemWatcher.New(path, filter);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileSystemWatcher).OnlyContainsMethodCall(
			nameof(IFileSystemWatcherFactory.New),
			path, filter);
	}

	[Fact]
	public async Task Method_Wrap_FileSystemWatcher_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		sut.Initialize();
		FileSystemWatcher fileSystemWatcher = new();

		using IFileSystemWatcher result = sut.FileSystemWatcher.Wrap(fileSystemWatcher);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.FileSystemWatcher).OnlyContainsMethodCall(
			nameof(IFileSystemWatcherFactory.Wrap),
			fileSystemWatcher);
	}

	[Fact]
	public async Task ToString_ShouldBeFileSystemWatcher()
	{
		IPathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher> sut
			= new MockFileSystem().Statistics.FileSystemWatcher;

		string? result = sut.ToString();

		await That(result).IsEqualTo("FileSystemWatcher");
	}
}

using FluentAssertions;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.FileSystemWatcher.Tests;

public class FileSystemWatcherTests
{
	[Fact]
	public void EnableRaisingEvents_ShouldTriggerWhenFileSystemChanges()
	{
		var fileSystem = new MockFileSystem();
		fileSystem.Initialize();
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			fileSystem.FileSystemWatcher.New(".");
		fileSystemWatcher.Created += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
		fileSystemWatcher.EnableRaisingEvents = true;

		fileSystem.Directory.CreateDirectory("foo");

		ms.Wait(1000).Should().BeTrue();

		result.Should().NotBeNull();
	}
}

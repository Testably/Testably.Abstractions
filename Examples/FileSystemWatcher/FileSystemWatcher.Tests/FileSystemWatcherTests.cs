using aweXpect;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.FileSystemWatcher.Tests;

public class FileSystemWatcherTests
{
	[Fact]
	public async Task EnableRaisingEvents_ShouldTriggerWhenFileSystemChanges()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithSubdirectory("foo");
		FileSystemEventArgs? result = null;
		ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			fileSystem.FileSystemWatcher.New(".");
		fileSystemWatcher.Deleted += (_, eventArgs) =>
		{
			result = eventArgs;
			ms.Set();
		};
		fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
		fileSystemWatcher.EnableRaisingEvents = true;

		fileSystem.Directory.Delete("foo");

		await Expect.That(ms.Wait(1000, TestContext.Current.CancellationToken)).IsTrue();

		await Expect.That(result).IsNotNull();
	}
}

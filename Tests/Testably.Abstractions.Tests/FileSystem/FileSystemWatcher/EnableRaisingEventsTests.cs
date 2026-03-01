using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public class EnableRaisingEventsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task EnableRaisingEvents_SetToFalse_ShouldStop(string path1, string path2)
	{
		FileSystem.Initialize().WithSubdirectory(path1).WithSubdirectory(path2);
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, _) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};
		fileSystemWatcher.EnableRaisingEvents = true;
		FileSystem.Directory.Delete(path1);
		await That(ms.Wait(ExpectSuccess, CancellationToken)).IsTrue();
		ms.Reset();

		fileSystemWatcher.EnableRaisingEvents = false;

		FileSystem.Directory.Delete(path2);
		await That(ms.Wait(ExpectTimeout, CancellationToken)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task EnableRaisingEvents_ShouldBeInitializedAsFalse(string path)
	{
		FileSystem.Initialize().WithSubdirectory(path);
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.Deleted += (_, _) =>
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		};

		FileSystem.Directory.Delete(path);

		await That(ms.Wait(ExpectTimeout, CancellationToken)).IsFalse();
	}
}

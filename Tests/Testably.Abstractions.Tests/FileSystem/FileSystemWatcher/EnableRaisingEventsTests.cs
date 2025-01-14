using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class EnableRaisingEventsTests
{
	[SkippableTheory]
	[AutoData]
	public void EnableRaisingEvents_SetToFalse_ShouldStop(string path1, string path2)
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
		ms.Wait(ExpectSuccess).Should().BeTrue();
		ms.Reset();

		fileSystemWatcher.EnableRaisingEvents = false;

		FileSystem.Directory.Delete(path2);
		ms.Wait(ExpectTimeout).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void EnableRaisingEvents_ShouldBeInitializedAsFalse(string path)
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

		ms.Wait(ExpectTimeout).Should().BeFalse();
	}
}

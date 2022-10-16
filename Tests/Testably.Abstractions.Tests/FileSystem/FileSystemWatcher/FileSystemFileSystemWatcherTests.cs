using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemFileSystemWatcherTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableTheory]
	[AutoData]
	public void BeginInit_ShouldStopListening(string path)
	{
		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;

		fileSystemWatcher.BeginInit();

		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		new Thread(() =>
		{
			while (!ms.IsSet)
			{
				TimeSystem.Thread.Sleep(10);
				FileSystem.Directory.CreateDirectory(path);
				FileSystem.Directory.Delete(path);
			}
		}).Start();
		IFileSystem.IFileSystemWatcher.IWaitForChangedResult result =
			fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, 100);

		ms.Set();
		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		result.TimedOut.Should().BeTrue();
		result.ChangeType.Should().Be(0);
		result.Name.Should().BeNull();
		result.OldName.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void EndInit_ShouldRestartListening(string path)
	{
		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;
		fileSystemWatcher.BeginInit();

		fileSystemWatcher.EndInit();

		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		new Thread(() =>
		{
			while (!ms.IsSet)
			{
				TimeSystem.Thread.Sleep(10);
				FileSystem.Directory.CreateDirectory(path);
				FileSystem.Directory.Delete(path);
			}
		}).Start();
		IFileSystem.IFileSystemWatcher.IWaitForChangedResult result =
			fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, 100);

		ms.Set();
		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		result.TimedOut.Should().BeFalse();
	}

	[SkippableTheory]
	[InlineData(-1, 4096)]
	[InlineData(4095, 4096)]
	[InlineData(4097, 4097)]
	public void InternalBufferSize_ShouldAtLeastHave4096Bytes(
		int bytes, int expectedBytes)
	{
		FileSystem.Initialize();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.InternalBufferSize = bytes;

		fileSystemWatcher.InternalBufferSize.Should().Be(expectedBytes);
	}

	[SkippableTheory]
	[AutoData]
	public void Path_SetToNotExistingPath_ShouldThrowArgumentException(string path)
	{
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			fileSystemWatcher.Path = path;
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain(path);
	}
}
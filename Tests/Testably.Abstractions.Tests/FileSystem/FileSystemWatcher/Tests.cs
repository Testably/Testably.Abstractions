using Moq;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void BeginInit_ShouldStopListening(string path)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;

		fileSystemWatcher.BeginInit();

		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		try
		{
			Task.Run(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});
			IFileSystemWatcher.IWaitForChangedResult result =
				fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, 1000);

			fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
			result.TimedOut.Should().BeTrue();
			result.ChangeType.Should().Be(0);
			result.Name.Should().BeNull();
			result.OldName.Should().BeNull();
		}
		finally
		{
			ms.Set();
		}
	}

	[SkippableFact]
	public void Container_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Container.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void EndInit_ShouldRestartListening(string path)
	{
		Test.SkipBrittleTestsOnRealFileSystem(FileSystem);

		FileSystem.Initialize();
		ManualResetEventSlim ms = new();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);
		fileSystemWatcher.EnableRaisingEvents = true;
		fileSystemWatcher.BeginInit();

		fileSystemWatcher.EndInit();

		fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
		try
		{
			Task.Run(() =>
			{
				while (!ms.IsSet)
				{
					Thread.Sleep(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});
			IFileSystemWatcher.IWaitForChangedResult result =
				fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created, 100);

			fileSystemWatcher.EnableRaisingEvents.Should().BeTrue();
			result.TimedOut.Should().BeFalse();
		}
		finally
		{
			ms.Set();
		}
	}

	[SkippableTheory]
	[InlineData(-1, 4096)]
	[InlineData(4095, 4096)]
	[InlineData(4097, 4097)]
	public void InternalBufferSize_ShouldAtLeastHave4096Bytes(
		int bytes, int expectedBytes)
	{
		FileSystem.Initialize();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.InternalBufferSize = bytes;

		fileSystemWatcher.InternalBufferSize.Should().Be(expectedBytes);
	}

	[SkippableTheory]
	[AutoData]
	public void Path_SetToNotExistingPath_ShouldThrowArgumentException(string path)
	{
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			fileSystemWatcher.Path = path;
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain(path);
	}

	[SkippableFact]
	public void Site_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Site.Should().BeNull();
	}

	[SkippableFact]
	public void Site_ShouldBeWritable()
	{
		ISite? site = new Mock<ISite>().Object;
		FileSystem.Initialize();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.Site = site;

		fileSystemWatcher.Site.Should().Be(site);
	}

	[SkippableFact]
	public void SynchronizingObject_ShouldBeInitializedWithNull()
	{
		FileSystem.Initialize();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.SynchronizingObject.Should().BeNull();
	}

	[SkippableFact]
	public void SynchronizingObject_ShouldBeWritable()
	{
		ISynchronizeInvoke? synchronizingObject = new Mock<ISynchronizeInvoke>().Object;
		FileSystem.Initialize();
		IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.SynchronizingObject = synchronizingObject;

		fileSystemWatcher.SynchronizingObject.Should().Be(synchronizingObject);
	}
}
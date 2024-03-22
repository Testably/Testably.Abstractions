using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class EventTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public async Task Changed_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.InitializeIn(BasePath);
		FileSystem.File.WriteAllText(path, "");
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e)
		{
			callCount++;
			ms.Set();
		}

		try
		{
			_ = Task.Run(async () =>
			{
				int i = 0;
				while (!ms.IsSet)
				{
					await Task.Delay(10);
					FileSystem.File.WriteAllText(path, i++.ToString(CultureInfo.InvariantCulture));
				}
			});

			fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms.Wait(10000);
			fileSystemWatcher.Changed -= FileSystemWatcherOnChanged;
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		await Task.Delay(10);
		FileSystem.File.WriteAllText(path, "foo");
		callCount.Should().Be(previousCallCount);
	}

	[SkippableTheory]
	[AutoData]
	public async Task Created_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.Initialize();
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
		{
			callCount++;
			ms.Set();
		}

		try
		{
			_ = Task.Run(async () =>
			{
				while (!ms.IsSet)
				{
					await Task.Delay(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});

			fileSystemWatcher.Created += FileSystemWatcherOnCreated;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms.Wait(10000);
			fileSystemWatcher.Created -= FileSystemWatcherOnCreated;
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		await Task.Delay(10);
		FileSystem.Directory.CreateDirectory("other" + path);
		FileSystem.Directory.Delete("other" + path);
		callCount.Should().Be(previousCallCount);
	}

	[SkippableTheory]
	[AutoData]
	public async Task Deleted_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.Initialize();
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnDeleted(object sender, FileSystemEventArgs e)
		{
			callCount++;
			ms.Set();
		}

		try
		{
			_ = Task.Run(async () =>
			{
				while (!ms.IsSet)
				{
					await Task.Delay(10);
					FileSystem.Directory.CreateDirectory(path);
					FileSystem.Directory.Delete(path);
				}
			});

			fileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms.Wait(10000);
			fileSystemWatcher.Deleted -= FileSystemWatcherOnDeleted;
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		await Task.Delay(10);
		FileSystem.Directory.CreateDirectory("other" + path);
		FileSystem.Directory.Delete("other" + path);
		callCount.Should().Be(previousCallCount);
	}

	[SkippableTheory]
	[AutoData]
	public async Task Renamed_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.InitializeIn(BasePath);
		FileSystem.File.WriteAllText(path, "");
		using ManualResetEventSlim ms = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnRenamed(object sender, FileSystemEventArgs e)
		{
			callCount++;
			ms.Set();
		}

		try
		{
			_ = Task.Run(async () =>
			{
				int i = 0;
				FileSystem.File.WriteAllText($"path-{i}", "");
				while (!ms.IsSet)
				{
					await Task.Delay(10);
					FileSystem.File.Move($"path-{i}", $"path-{++i}");
				}
			});

			fileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms.Wait(10000);
			fileSystemWatcher.Renamed -= FileSystemWatcherOnRenamed;
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		await Task.Delay(10);
		FileSystem.File.Move(path, "other-path");
		callCount.Should().Be(previousCallCount);
	}
}

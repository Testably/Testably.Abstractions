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
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}

		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					int i = 0;
					while (!ms.IsSet)
					{
						FileSystem.File.WriteAllText(path,
							i++.ToString(CultureInfo.InvariantCulture));
						await Task.Delay(10);
					}
				}
				catch (IOException)
				{
					// Ignore any IOException
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
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
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}

		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
						await Task.Delay(10);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
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
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}

		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					while (!ms.IsSet)
					{
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
						await Task.Delay(10);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
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
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms.Set();
			}
			catch (ObjectDisposedException)
			{
				// Ignore any ObjectDisposedException
			}
		}

		try
		{
			_ = Task.Run(async () =>
			{
				// ReSharper disable once AccessToDisposedClosure
				try
				{
					int i = 0;
					FileSystem.File.WriteAllText($"path-{i}", "");
					while (!ms.IsSet)
					{
						FileSystem.File.Move($"path-{i}", $"path-{++i}");
						await Task.Delay(10);
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
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

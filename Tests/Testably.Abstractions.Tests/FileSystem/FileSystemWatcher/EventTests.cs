using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable MethodSupportsCancellation
// ReSharper disable MethodHasAsyncOverloadWithCancellation

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class EventTests
{
	[Theory]
	[AutoData]
	public void Changed_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.InitializeIn(BasePath);
		FileSystem.File.WriteAllText(path, "");
		using CancellationTokenSource cts = new(ExpectSuccess);
		CancellationToken token = cts.Token;
		using ManualResetEventSlim ms1 = new();
		using ManualResetEventSlim ms2 = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e)
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms2.Set();
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
					while (!token.IsCancellationRequested)
					{
						string content = i++.ToString(CultureInfo.InvariantCulture);
						FileSystem.File.WriteAllText(path, content);
						await Task.Delay(10, token);
						ms1.Set();
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
			}, token);

			fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms2.Wait(ExpectSuccess, token).Should().BeTrue();
			fileSystemWatcher.Changed -= FileSystemWatcherOnChanged;
			ms1.Reset();
			ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		callCount.Should().Be(previousCallCount);
		cts.Cancel();
	}

	[Theory]
	[AutoData]
	public void Created_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.Initialize();
		using CancellationTokenSource cts = new(ExpectSuccess);
		CancellationToken token = cts.Token;
		using ManualResetEventSlim ms1 = new();
		using ManualResetEventSlim ms2 = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms2.Set();
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
					while (!token.IsCancellationRequested)
					{
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
						await Task.Delay(10, token);
						ms1.Set();
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, token);

			fileSystemWatcher.Created += FileSystemWatcherOnCreated;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms2.Wait(ExpectSuccess, token).Should().BeTrue();
			fileSystemWatcher.Created -= FileSystemWatcherOnCreated;
			ms1.Reset();
			ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		FileSystem.Directory.CreateDirectory("other" + path);
		FileSystem.Directory.Delete("other" + path);
		callCount.Should().Be(previousCallCount);
		cts.Cancel();
	}

	[Theory]
	[AutoData]
	public void Deleted_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.Initialize();
		using CancellationTokenSource cts = new(ExpectSuccess);
		CancellationToken token = cts.Token;
		using ManualResetEventSlim ms1 = new();
		using ManualResetEventSlim ms2 = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnDeleted(object sender, FileSystemEventArgs e)
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms2.Set();
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
					while (!token.IsCancellationRequested)
					{
						FileSystem.Directory.CreateDirectory(path);
						FileSystem.Directory.Delete(path);
						await Task.Delay(10, token);
						ms1.Set();
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, token);

			fileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms2.Wait(ExpectSuccess, token).Should().BeTrue();
			fileSystemWatcher.Deleted -= FileSystemWatcherOnDeleted;
			ms1.Reset();
			ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		FileSystem.Directory.CreateDirectory("other" + path);
		FileSystem.Directory.Delete("other" + path);
		callCount.Should().Be(previousCallCount);
		cts.Cancel();
	}

	[Theory]
	[AutoData]
	public void Renamed_ShouldTriggerUntilEventIsRemoved(string path)
	{
		int callCount = 0;
		FileSystem.InitializeIn(BasePath);
		FileSystem.File.WriteAllText(path, "");
		using CancellationTokenSource cts = new(ExpectSuccess);
		CancellationToken token = cts.Token;
		using ManualResetEventSlim ms1 = new();
		using ManualResetEventSlim ms2 = new();
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		void FileSystemWatcherOnRenamed(object sender, FileSystemEventArgs e)
		{
			// ReSharper disable once AccessToDisposedClosure
			try
			{
				callCount++;
				ms2.Set();
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
					while (!token.IsCancellationRequested)
					{
						FileSystem.File.Move($"path-{i}", $"path-{++i}");
						await Task.Delay(10, token);
						ms1.Set();
					}
				}
				catch (ObjectDisposedException)
				{
					// Ignore any ObjectDisposedException
				}
			}, token);

			fileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;
			fileSystemWatcher.EnableRaisingEvents = true;
		}
		finally
		{
			ms2.Wait(ExpectSuccess, token).Should().BeTrue();
			fileSystemWatcher.Renamed -= FileSystemWatcherOnRenamed;
			ms1.Reset();
			ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		}

		callCount.Should().BeGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		ms1.Wait(ExpectSuccess, token).Should().BeTrue();
		FileSystem.File.Move(path, "other-path");
		callCount.Should().Be(previousCallCount);
		cts.Cancel();
	}
}

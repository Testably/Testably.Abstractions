using System.Globalization;
using System.IO;
using System.Threading;
// ReSharper disable MethodSupportsCancellation
// ReSharper disable MethodHasAsyncOverloadWithCancellation

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public partial class EventTests
{
	[Theory]
	[AutoData]
	public async Task Changed_ShouldTriggerUntilEventIsRemoved(string path)
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
			await That(ms2.Wait(ExpectSuccess, token)).IsTrue();
			fileSystemWatcher.Changed -= FileSystemWatcherOnChanged;
			ms1.Reset();
			await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		}

		await That(callCount).IsGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		await That(callCount).IsEqualTo(previousCallCount);
		cts.Cancel();
	}

	[Theory]
	[AutoData]
	public async Task Created_ShouldTriggerUntilEventIsRemoved(string path)
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
			await That(ms2.Wait(ExpectSuccess, token)).IsTrue();
			fileSystemWatcher.Created -= FileSystemWatcherOnCreated;
			ms1.Reset();
			await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		}

		await That(callCount).IsGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		FileSystem.Directory.CreateDirectory("other" + path);
		FileSystem.Directory.Delete("other" + path);
		await That(callCount).IsEqualTo(previousCallCount);
		cts.Cancel();
	}

	[Theory]
	[AutoData]
	public async Task Deleted_ShouldTriggerUntilEventIsRemoved(string path)
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
			await That(ms2.Wait(ExpectSuccess, token)).IsTrue();
			fileSystemWatcher.Deleted -= FileSystemWatcherOnDeleted;
			ms1.Reset();
			await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		}

		await That(callCount).IsGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		FileSystem.Directory.CreateDirectory("other" + path);
		FileSystem.Directory.Delete("other" + path);
		await That(callCount).IsEqualTo(previousCallCount);
		cts.Cancel();
	}

	[Theory]
	[AutoData]
	public async Task Renamed_ShouldTriggerUntilEventIsRemoved(string path)
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
			await That(ms2.Wait(ExpectSuccess, token)).IsTrue();
			fileSystemWatcher.Renamed -= FileSystemWatcherOnRenamed;
			ms1.Reset();
			await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		}

		await That(callCount).IsGreaterThanOrEqualTo(1);
		int previousCallCount = callCount;

		ms1.Reset();
		await That(ms1.Wait(ExpectSuccess, token)).IsTrue();
		FileSystem.File.Move(path, "other-path");
		await That(callCount).IsEqualTo(previousCallCount);
		cts.Cancel();
	}
}

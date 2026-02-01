using aweXpect.Synchronous;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class ChangeHandlerTests(ITestOutputHelper testOutputHelper)
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Theory]
	[AutoData]
	public async Task CreateDirectory_CustomException_ShouldNotCreateDirectory(
		string path, Exception exceptionToThrow)
	{
		FileSystem.Intercept.Event(_ =>
		{
			Synchronously.Verify(That(FileSystem.Directory.Exists(path)).IsFalse());
			throw exceptionToThrow;
		});
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		await That(FileSystem.Directory.Exists(path)).IsFalse();
		await That(exception).IsEqualTo(exceptionToThrow);
	}

	[Theory]
	[AutoData]
	public async Task CreateDirectory_CustomException_ShouldOnlyTriggerChangeOccurring(
		string path, Exception exceptionToThrow)
	{
		string? receivedPath = null;
		FileSystem.Intercept.Event(_ => throw exceptionToThrow);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		await That(exception).IsEqualTo(exceptionToThrow);
		await That(receivedPath!).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task
		CreateDirectory_WithParentDirectories_ShouldTriggerNotificationForEachDirectory(
			string path1, string path2, string path3)
	{
		FileSystem.Initialize();

		string path = FileSystem.Path.Combine(path1, path2, path3);
		int eventCount = 0;
		FileSystem.Notify
			.OnEvent(c =>
				{
					testOutputHelper.WriteLine($"Received event {c}");
					eventCount++;
				},
				c => c.ChangeType == WatcherChangeTypes.Created)
			.ExecuteWhileWaiting(() =>
			{
				FileSystem.Directory.CreateDirectory(path);
			})
			.Wait(count: 3);

		await That(eventCount).IsEqualTo(3);
	}

	[Theory]
	[MemberData(nameof(NotificationTriggeringMethods))]
	public async Task ExecuteCallback_ShouldTriggerNotification(
		Action<IFileSystem, string>? initialization,
		Action<IFileSystem, string> callback,
		WatcherChangeTypes expectedChangeType,
		FileSystemTypes expectedFileSystemType,
		string path)
	{
		string? receivedPath = null;
		FileSystem.Initialize();
		initialization?.Invoke(FileSystem, path);

		FileSystem.Notify
			.OnEvent(c => receivedPath = c.Path,
				c => c.ChangeType == expectedChangeType &&
				     c.FileSystemType == expectedFileSystemType)
			.ExecuteWhileWaiting(() =>
			{
				callback.Invoke(FileSystem, path);
			})
			.Wait();

		await That(receivedPath).IsEqualTo(FileSystem.Path.GetFullPath(path));
	}

	[Fact]
	public async Task Watcher_ShouldNotTriggerWhenFileSystemWatcherDoesNotMatch()
	{
		FileSystem.Directory.CreateDirectory("bar");
		IFileSystemWatcher watcher = FileSystem.FileSystemWatcher.New("bar");
		watcher.EnableRaisingEvents = true;

		IAwaitableCallback<ChangeDescription> onEvent = FileSystem.Watcher.OnTriggered();

		void Act() =>
			onEvent.Wait(timeout: 100,
				executeWhenWaiting: () => FileSystem.File.WriteAllText(@"foo.txt", "some-text"));

		await That(Act).Throws<TimeoutException>();
	}

	[Fact]
	public async Task Watcher_ShouldTriggerWhenFileSystemWatcherSendsNotification()
	{
		bool isTriggered = false;
		IFileSystemWatcher watcher = FileSystem.FileSystemWatcher.New(".");
		watcher.Created += (_, __) => isTriggered = true;
		watcher.EnableRaisingEvents = true;

		IAwaitableCallback<ChangeDescription> onEvent = FileSystem.Watcher.OnTriggered();

		onEvent.Wait(timeout: 1000,
			executeWhenWaiting: () => FileSystem.File.WriteAllText(@"foo.txt", "some-text"));

		await That(isTriggered).IsTrue();
	}

	#region Helpers

	public static
		TheoryData<
			Action<IFileSystem, string>?,
			Action<IFileSystem, string>,
			WatcherChangeTypes,
			FileSystemTypes,
			string> NotificationTriggeringMethods()
	{
		return new TheoryData<
			Action<IFileSystem, string>?,
			Action<IFileSystem, string>,
			WatcherChangeTypes,
			FileSystemTypes,
			string>
		{
			{
				null, (f, p) => f.Directory.CreateDirectory(p), WatcherChangeTypes.Created,
				FileSystemTypes.Directory, $"path_{Guid.NewGuid()}"
			},
			{
				(f, p) => f.Directory.CreateDirectory(p), (f, p) => f.Directory.Delete(p),
				WatcherChangeTypes.Deleted, FileSystemTypes.Directory, $"path_{Guid.NewGuid()}"
			},
			{
				null, (f, p) => f.File.WriteAllText(p, null), WatcherChangeTypes.Created,
				FileSystemTypes.File, $"path_{Guid.NewGuid()}"
			},
			{
				(f, p) => f.File.WriteAllText(p, null), (f, p) => f.File.Delete(p),
				WatcherChangeTypes.Deleted, FileSystemTypes.File, $"path_{Guid.NewGuid()}"
			},
		};
	}

	#endregion
}

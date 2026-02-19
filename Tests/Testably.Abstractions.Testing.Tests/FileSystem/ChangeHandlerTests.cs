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
		using IAwaitableCallback<ChangeDescription> onEvent = FileSystem.Notify
			.OnEvent(c =>
				{
					testOutputHelper.WriteLine($"Received event {c}");
					eventCount++;
				},
				c => c.ChangeType == WatcherChangeTypes.Created);
		FileSystem.Directory.CreateDirectory(path);

		onEvent.Wait(3);

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

		using IAwaitableCallback<ChangeDescription> onEvent = FileSystem.Notify
			.OnEvent(c => receivedPath = c.Path,
				c => c.ChangeType == expectedChangeType &&
				     c.FileSystemType == expectedFileSystemType);

		callback.Invoke(FileSystem, path);

		onEvent.Wait();

		await That(receivedPath).IsEqualTo(FileSystem.Path.GetFullPath(path));
	}

	[Fact]
	public async Task Watcher_ShouldNotTriggerWhenFileSystemWatcherDoesNotMatch()
	{
		FileSystem.Directory.CreateDirectory("bar");
		IFileSystemWatcher watcher = FileSystem.FileSystemWatcher.New("bar");
		watcher.EnableRaisingEvents = true;

		using IAwaitableCallback<ChangeDescription> onEvent = FileSystem.Watcher.OnTriggered();

		FileSystem.File.WriteAllText(@"foo.txt", "some-text");

		void Act() =>
			// ReSharper disable once AccessToDisposedClosure
			onEvent.Wait(timeout: 100);

		await That(Act).Throws<TimeoutException>();
	}

	[Fact]
	public async Task Watcher_ShouldTriggerWhenFileSystemWatcherSendsNotification()
	{
		bool isTriggered = false;
		FileSystem.InitializeIn(".");
		IFileSystemWatcher watcher = FileSystem.FileSystemWatcher.New(".");
		watcher.Created += (_, _) => isTriggered = true;
		watcher.EnableRaisingEvents = true;

		using IAwaitableCallback<ChangeDescription> onEvent = FileSystem.Watcher.OnTriggered();
		FileSystem.File.WriteAllText(@"foo.txt", "some-text");

		onEvent.Wait(timeout: 5000);

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

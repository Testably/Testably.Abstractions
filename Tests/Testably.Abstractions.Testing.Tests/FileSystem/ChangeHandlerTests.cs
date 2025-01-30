using System.IO;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class ChangeHandlerTests(ITestOutputHelper testOutputHelper)
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Theory]
	[AutoData]
	public void CreateDirectory_CustomException_ShouldNotCreateDirectory(
		string path, Exception exceptionToThrow)
	{
		FileSystem.Intercept.Event(_ =>
		{
			FileSystem.Directory.Exists(path).Should().BeFalse();
			throw exceptionToThrow;
		});
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		FileSystem.Directory.Exists(path).Should().BeFalse();
		exception.Should().Be(exceptionToThrow);
	}

	[Theory]
	[AutoData]
	public void CreateDirectory_CustomException_ShouldOnlyTriggerChangeOccurring(
		string path, Exception exceptionToThrow)
	{
		string? receivedPath = null;
		FileSystem.Intercept.Event(_ => throw exceptionToThrow);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().Be(exceptionToThrow);
		receivedPath!.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void
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

		eventCount.Should().Be(3);
	}

	[Theory]
	[MemberData(nameof(NotificationTriggeringMethods))]
	public void ExecuteCallback_ShouldTriggerNotification(
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

		receivedPath.Should().Be(FileSystem.Path.GetFullPath(path));
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

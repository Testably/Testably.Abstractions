using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests;

public class FileSystemMockNotificationTests
{
	public FileSystemMock FileSystem { get; }
	private readonly ITestOutputHelper _testOutputHelper;

	public FileSystemMockNotificationTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		FileSystem = new FileSystemMock();

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Notify]
	public void
		CreateDirectory_WithParentDirectories_ShouldTriggerNotificationForEachDirectory(
			string path1, string path2, string path3)
	{
		FileSystem.Initialize();

		string path = FileSystem.Path.Combine(path1, path2, path3);
		int eventCount = 0;
		FileSystem.Notify
		   .OnChange(c =>
				{
					_testOutputHelper.WriteLine($"Received event {c}");
					eventCount++;
				},
				c => c.ChangeType == WatcherChangeTypes.Created)
		   .Execute(() =>
			{
				FileSystem.Directory.CreateDirectory(path);
			})
		   .Wait(count: 3);

		eventCount.Should().Be(3);
	}

	[SkippableTheory]
	[MemberData(nameof(NotificationTriggeringMethods))]
	[FileSystemTests.Notify]
	public void ExecuteCallback_ShouldTriggerNotification(
		Action<IFileSystem, string>? initialization,
		Action<IFileSystem, string> callback,
		WatcherChangeTypes expectedChangeType,
		FileSystemMock.FileSystemTypes expectedFileSystemType,
		string path)
	{
		string? receivedPath = null;
		initialization?.Invoke(FileSystem, path);

		FileSystem.Notify
		   .OnChange(c => receivedPath = c.Path,
				c => c.ChangeType == expectedChangeType &&
				     c.FileSystemType == expectedFileSystemType)
		   .Execute(() =>
			{
				callback.Invoke(FileSystem, path);
			})
		   .Wait();

		receivedPath.Should().Be(FileSystem.Path.GetFullPath(path));
	}

	public static IEnumerable<object?[]> NotificationTriggeringMethods()
	{
		yield return new object?[]
		{
			null,
			new Action<IFileSystem, string>((f, p) => f.Directory.CreateDirectory(p)),
			WatcherChangeTypes.Created, FileSystemMock.FileSystemTypes.Directory,
			$"path_{Guid.NewGuid()}"
		};
		yield return new object?[]
		{
			null,
			new Action<IFileSystem, string>((f, p) => f.File.WriteAllText(p, null)),
			WatcherChangeTypes.Created, FileSystemMock.FileSystemTypes.File,
			$"path_{Guid.NewGuid()}"
		};
	}
}
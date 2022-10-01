using System.Collections.Generic;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests;

public class FileSystemMockNotificationTests
{
    #region Test Setup

    public FileSystemMock FileSystem { get; }
    private readonly ITestOutputHelper _testOutputHelper;

    public FileSystemMockNotificationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        FileSystem = new FileSystemMock();
    }

    #endregion

    [Theory]
    [AutoData]
    [FileSystemTests.Notify]
    public void
        CreateDirectory_WithParentDirectories_ShouldTriggerNotificationForEachDirectory(
            string path1, string path2, string path3)
    {
        string path = FileSystem.Path.Combine(path1, path2, path3);
        int eventCount = 0;
        Notification.IAwaitableCallback<FileSystemMock.ChangeDescription>
            awaitable = FileSystem.Notify.OnChange(c =>
                {
                    _testOutputHelper.WriteLine($"Received event {c}");
                    eventCount++;
                },
                c => c.Type == FileSystemMock.ChangeTypes.DirectoryCreated);

        FileSystem.Directory.CreateDirectory(path);

        awaitable.Wait(count: 3);

        eventCount.Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(NotificationTriggeringMethods))]
    [FileSystemTests.Notify]
    public void ExecuteCallback_ShouldTriggerNotification(
        Action<IFileSystem, string>? initialization,
        Action<IFileSystem, string> callback,
        FileSystemMock.ChangeTypes expectedChangeType,
        string path)
    {
        string? receivedPath = null;
        initialization?.Invoke(FileSystem, path);

        FileSystem.Notify
           .OnChange(c => receivedPath = c.Path,
                c => c.Type == expectedChangeType)
           .Execute(() =>
            {
                callback.Invoke(FileSystem, path);
            })
           .Wait();

        receivedPath.Should().Be(FileSystem.Path.GetFullPath(path));
    }

    #region Helpers

    public static IEnumerable<object?[]> NotificationTriggeringMethods()
    {
        yield return new object?[]
        {
            null,
            new Action<IFileSystem, string>((f, p) => f.Directory.CreateDirectory(p)),
            FileSystemMock.ChangeTypes.DirectoryCreated, $"path_{Guid.NewGuid()}"
        };
        yield return new object?[]
        {
            null,
            new Action<IFileSystem, string>((f, p) => f.File.WriteAllText(p, null)),
            FileSystemMock.ChangeTypes.FileCreated, $"path_{Guid.NewGuid()}"
        };
    }

    #endregion
}
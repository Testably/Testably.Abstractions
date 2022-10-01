using System.Collections.Generic;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests;

public class FileSystemCallbackHandlerTests
{
    #region Test Setup

    public FileSystemMock FileSystem { get; }
    private readonly ITestOutputHelper _testOutputHelper;

    public FileSystemCallbackHandlerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        FileSystem = new FileSystemMock();
    }

    #endregion

    [Theory]
    [AutoData]
    [FileSystemTests.CallbackHandler(
        nameof(FileSystemMock.CallbackChangeTypes.Created))]
    public void CreateDirectory_CustomException_ShouldOnlyTriggerChangeOccurring(
        string path, Exception exceptionToThrow)
    {
        string? receivedPath = null;
        FileSystem.Intercept.Change(_ => throw exceptionToThrow);
        Exception? exception = FileSystem.Notify
           .OnChange(c => receivedPath = c.Path)
           .Execute(() =>
            {
                return Record.Exception(() =>
                {
                    FileSystem.Directory.CreateDirectory(path);
                });
            })
           .Wait(timeout: 50);

        exception.Should().Be(exceptionToThrow);
        receivedPath.Should().BeNull();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.CallbackHandler(
        nameof(FileSystemMock.CallbackChangeTypes.Created))]
    public void
        CreateDirectory_WithParentDirectories_ShouldTriggerNotificationForEachDirectory(
            string path1, string path2, string path3)
    {
        string path = FileSystem.Path.Combine(path1, path2, path3);
        int eventCount = 0;
        Notification.IAwaitableCallback<FileSystemMock.CallbackChange>
            awaitable = FileSystem.Notify.OnChange(c =>
                {
                    _testOutputHelper.WriteLine($"Received event {c}");
                    eventCount++;
                },
                c => c.Type == FileSystemMock.CallbackChangeTypes.DirectoryCreated);

        FileSystem.Directory.CreateDirectory(path);

        awaitable.Wait(count: 3);

        eventCount.Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(NotificationTriggeringMethods))]
    [FileSystemTests.CallbackHandler(
        nameof(FileSystemMock.CallbackChangeTypes.Created))]
    public void ExecuteCallback_ShouldTriggerNotification(
        Action<IFileSystem, string>? initialization,
        Action<IFileSystem, string> callback,
        FileSystemMock.CallbackChangeTypes expectedChangeType,
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
            FileSystemMock.CallbackChangeTypes.DirectoryCreated,
            $"path_{Guid.NewGuid()}"
        };
        yield return new object?[]
        {
            null,
            new Action<IFileSystem, string>((f, p) => f.File.WriteAllText(p, null)),
            FileSystemMock.CallbackChangeTypes.FileCreated, $"path_{Guid.NewGuid()}"
        };
    }

    #endregion
}
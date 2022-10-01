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
        nameof(FileSystemMock.CallbackChangeType.Created))]
    public void CreateDirectory_ShouldTriggerNotification(string path)
    {
        string? receivedPath = null;
        Notification.IAwaitableCallback<FileSystemMock.CallbackChange>
            awaitable = FileSystem.On.ChangeOccurred(c => receivedPath = c.Path,
                c => c.Type == FileSystemMock.CallbackChangeType.DirectoryCreated);

        FileSystem.Directory.CreateDirectory(path);

        awaitable.Wait();

        receivedPath.Should().Be(FileSystem.Path.GetFullPath(path));
    }

    [Theory]
    [AutoData]
    [FileSystemTests.CallbackHandler(
        nameof(FileSystemMock.CallbackChangeType.Created))]
    public void CreateDirectory_CustomException_ShouldOnlyTriggerChangeOccurring(
        string path, Exception exceptionToThrow)
    {
        string? receivedPath = null;
        Notification.IAwaitableCallback<FileSystemMock.CallbackChange>
            awaitable = FileSystem.On
               .ChangeOccurring(_ => throw exceptionToThrow)
               .ChangeOccurred(c => receivedPath = c.Path);

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.CreateDirectory(path);
        });

        awaitable.Wait(timeout: 50);

        exception.Should().Be(exceptionToThrow);
        receivedPath.Should().BeNull();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.CallbackHandler(
        nameof(FileSystemMock.CallbackChangeType.Created))]
    public void
        CreateDirectory_WithParentDirectories_ShouldTriggerNotificationForEachDirectory(
            string path1, string path2, string path3)
    {
        string path = FileSystem.Path.Combine(path1, path2, path3);
        int eventCount = 0;
        Notification.IAwaitableCallback<FileSystemMock.CallbackChange>
            awaitable = FileSystem.On.ChangeOccurred(c =>
                {
                    _testOutputHelper.WriteLine($"Received event {c}");
                    eventCount++;
                },
                c => c.Type == FileSystemMock.CallbackChangeType.DirectoryCreated);

        FileSystem.Directory.CreateDirectory(path);

        awaitable.Wait(count: 3);

        eventCount.Should().Be(3);
    }
}
namespace Testably.Abstractions.Tests;

public class FileSystemMockInterceptionTests
{
    #region Test Setup

    public FileSystemMock FileSystem { get; }

    public FileSystemMockInterceptionTests()
    {
        FileSystem = new FileSystemMock();
    }

    #endregion

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Intercept]
    public void CreateDirectory_CustomException_ShouldOnlyTriggerChangeOccurring(
        string path, Exception exceptionToThrow)
    {
        string? receivedPath = null;
        FileSystem.Intercept.Change(_ => throw exceptionToThrow);
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Notify
               .OnChange(c => receivedPath = c.Path)
               .Execute(() =>
                {
                    FileSystem.Directory.CreateDirectory(path);
                })
               .Wait(timeout: 500);
        });

        exception.Should().Be(exceptionToThrow);
        receivedPath.Should().BeNull();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.Intercept]
    public void CreateDirectory_CustomException_ShouldNotCreateDirectory(
        string path, Exception exceptionToThrow)
    {
        FileSystem.Intercept.Change(_ =>
        {
            FileSystem.Directory.Exists(path).Should().BeFalse();
            throw exceptionToThrow;
        });
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Notify
               .OnChange()
               .Execute(() =>
                {
                    FileSystem.Directory.CreateDirectory(path);
                })
               .Wait(timeout: 500);
        });

        FileSystem.Directory.Exists(path).Should().BeFalse();
        exception.Should().Be(exceptionToThrow);
    }
}
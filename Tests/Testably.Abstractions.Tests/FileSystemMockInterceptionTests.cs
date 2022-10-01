using Xunit.Abstractions;

namespace Testably.Abstractions.Tests;

public class FileSystemMockInterceptionTests
{
    #region Test Setup

    public FileSystemMock FileSystem { get; }
    private readonly ITestOutputHelper _testOutputHelper;

    public FileSystemMockInterceptionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        FileSystem = new FileSystemMock();
    }

    #endregion

    [Theory]
    [AutoData]
    [FileSystemTests.Intercept]
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
}
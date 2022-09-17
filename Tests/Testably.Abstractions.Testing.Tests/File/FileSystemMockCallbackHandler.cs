using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.File;

public class FileSystemMockCallbackHandler
{
    [Fact]
    public void DummyTest_AsCallbacksAreNotYetImplemented()
    {
        FileSystemMock fileSystem = new();
        FileSystemMock.ICallbackHandler result = fileSystem.On;

        result.Should().NotBeNull();
    }
}
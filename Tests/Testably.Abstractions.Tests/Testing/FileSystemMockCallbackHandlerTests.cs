using FluentAssertions;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Tests.Testing;

public class FileSystemMockCallbackHandlerTests
{
    [Fact]
    public void DummyTest_AsCallbacksAreNotYetImplemented()
    {
        FileSystemMock fileSystem = new();
        FileSystemMock.ICallbackHandler result = fileSystem.On;

        result.Should().NotBeNull();
    }
}
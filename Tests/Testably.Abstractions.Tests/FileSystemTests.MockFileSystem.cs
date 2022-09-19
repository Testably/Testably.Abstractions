using Testably.Abstractions.Testing;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class MockFileSystem : FileSystemTests
    {
        public MockFileSystem() : base(new FileSystemMock())
        {
        }
    }
}
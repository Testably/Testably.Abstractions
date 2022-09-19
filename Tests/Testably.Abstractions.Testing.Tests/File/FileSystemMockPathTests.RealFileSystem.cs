namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockPathTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealFileSystem : FileSystemMockPathTests
    {
        public RealFileSystem() : base(new FileSystem())
        {
        }
    }
}
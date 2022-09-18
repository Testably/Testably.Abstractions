namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockPathTests
{
    // ReSharper disable once UnusedMember.Global
    public class RealFileSystem : FileSystemMockPathTests
    {
        public RealFileSystem() : base(new FileSystem())
        {
        }
    }
}
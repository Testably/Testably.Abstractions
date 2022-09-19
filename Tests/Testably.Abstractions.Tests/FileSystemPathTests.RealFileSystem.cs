namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests
{
    // ReSharper disable once UnusedMember.Global
    [Collection(FileTestHelper.RealFileSystemCollection)]
    public sealed class RealFileSystem : FileSystemPathTests<FileSystem>
    {
        public RealFileSystem() : base(new FileSystem())
        {
        }
    }
}
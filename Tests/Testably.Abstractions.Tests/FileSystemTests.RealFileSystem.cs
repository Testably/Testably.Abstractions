namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemTests
{
    // ReSharper disable once UnusedMember.Global
    [Collection(FileTestHelper.RealFileSystemCollection)]
    public sealed class RealFileSystem : FileSystemTests<FileSystem>
    {
        public RealFileSystem() : base(new FileSystem())
        {
        }
    }
}
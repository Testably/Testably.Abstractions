namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealFileSystem : FileSystemPathTests<FileSystem>
    {
        public RealFileSystem() : base(new FileSystem())
        {
        }
    }
}
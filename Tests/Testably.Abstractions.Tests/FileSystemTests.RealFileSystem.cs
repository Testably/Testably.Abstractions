namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealFileSystem : FileSystemTests
    {
        public RealFileSystem() : base(new FileSystem())
        {
        }
    }
}
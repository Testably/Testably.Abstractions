using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public TFileSystem FileSystem { get; }

    protected FileSystemTests(TFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    #endregion

    [Fact]
    [FileSystemTests.ExtensionPoint]
    public void Directory_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.Directory.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    [FileSystemTests.ExtensionPoint]
    public void DirectoryInfo_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.DirectoryInfo.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    [FileSystemTests.ExtensionPoint]
    public void File_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.File.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    [FileSystemTests.ExtensionPoint]
    public void FileInfo_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.FileInfo.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    [FileSystemTests.ExtensionPoint]
    public void Path_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.Path.FileSystem;

        result.Should().Be(FileSystem);
    }
}

/// <summary>
///     Attributes for <see cref="FileSystemTests{TFileSystem}" />
/// </summary>
public static class FileSystemTests
{
    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IFile" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class ExtensionPoint : TestabilityTraitAttribute
    {
        public ExtensionPoint() : base(nameof(IFileSystem),
            nameof(IFileSystem.IFileSystemExtensionPoint), null)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IFile" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class File : TestabilityTraitAttribute
    {
        public File(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IFile), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IFileInfoFactory" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class FileInfo : TestabilityTraitAttribute
    {
        public FileInfo(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IFileInfoFactory), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IPath" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class Path : TestabilityTraitAttribute
    {
        public Path(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IPath),
            method)
        {
        }
    }
}
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
    ///     Tests for methods in <see cref="IFileSystem.IDirectory" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class Directory : TestabilityTraitAttribute
    {
        public Directory(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IDirectory), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IDirectoryInfo" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class DirectoryInfo : TestabilityTraitAttribute
    {
        public DirectoryInfo(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IDirectoryInfo), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IDirectoryInfoFactory" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class DirectoryInfoFactory : TestabilityTraitAttribute
    {
        public DirectoryInfoFactory(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IDirectoryInfoFactory), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IDriveInfo" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class DriveInfo : TestabilityTraitAttribute
    {
        public DriveInfo(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IDriveInfo), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IDriveInfoFactory" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class DriveInfoFactory : TestabilityTraitAttribute
    {
        public DriveInfoFactory(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IDriveInfoFactory), method)
        {
        }
    }

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
    ///     Tests for methods in <see cref="IFileSystem.IFileInfo" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class FileInfo : TestabilityTraitAttribute
    {
        public FileInfo(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IFileInfo), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IFileInfoFactory" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class FileInfoFactory : TestabilityTraitAttribute
    {
        public FileInfoFactory(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IFileInfoFactory), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="FileSystemStream" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class FileStream : TestabilityTraitAttribute
    {
        public FileStream(string? method = null) : base(nameof(IFileSystem),
            nameof(FileSystemStream), method ?? " (other) ")
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IFileStreamFactory" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class FileStreamFactory : TestabilityTraitAttribute
    {
        public FileStreamFactory(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IFileStreamFactory), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="IFileSystem.IFileSystemInfo" /> in <see cref="IFileSystem" />.
    /// </summary>
    public class FileSystemInfo : TestabilityTraitAttribute
    {
        public FileSystemInfo(string method) : base(nameof(IFileSystem),
            nameof(IFileSystem.IFileSystemInfo), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="FileSystemMock.IInterceptionHandler" /> in <see cref="FileSystemMock" />.
    /// </summary>
    public class Intercept : TestabilityTraitAttribute
    {
        public Intercept(string? method = null) : base(nameof(FileSystemMock),
            nameof(FileSystemMock.Intercept), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="FileSystemMock.INotificationHandler" /> in <see cref="FileSystemMock" />.
    /// </summary>
    public class Notify : TestabilityTraitAttribute
    {
        public Notify(string? method = null) : base(nameof(FileSystemMock),
            nameof(FileSystemMock.Notify), method)
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
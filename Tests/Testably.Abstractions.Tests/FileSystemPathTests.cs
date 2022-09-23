using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public TFileSystem FileSystem { get; }

    protected FileSystemPathTests(TFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    #endregion

    [Fact]
    public void AltDirectorySeparatorChar_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.AltDirectorySeparatorChar;

        result.Should().Be(Path.AltDirectorySeparatorChar);
    }

    [Fact]
    public void DirectorySeparatorChar_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.DirectorySeparatorChar;

        result.Should().Be(Path.DirectorySeparatorChar);
    }

    [Fact]
    public void PathSeparator_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.PathSeparator;

        result.Should().Be(Path.PathSeparator);
    }

    [Fact]
    public void VolumeSeparatorChar_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.VolumeSeparatorChar;

        result.Should().Be(Path.VolumeSeparatorChar);
    }
}
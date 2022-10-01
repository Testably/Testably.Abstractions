using System.IO;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllBytes))]
    public void WriteAllBytes_MissingDrive_ShouldThrowDirectoryNotFoundException(
        string path, byte[] contents)
    {
        Skip.IfNot(Test.RunsOnWindows);

        IFileSystem.IDriveInfo driveInfo = GetUnmappedDrive();

        path = $"{driveInfo.Name}{path}";

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.WriteAllBytes(path, contents);
        });

        exception.Should().BeOfType<DirectoryNotFoundException>()
           .Which.Message.Should().Contain($"'{path}'");
    }

    private IFileSystem.IDriveInfo GetUnmappedDrive()
    {
        IFileSystem.IDriveInfo? driveInfo = null;
        for (char c = 'A'; c <= 'Z'; c++)
        {
            driveInfo = FileSystem.DriveInfo.New($"{c}");
            if (FileSystem.DriveInfo.GetDrives().All(d => d.Name != driveInfo.Name))
            {
                break;
            }
        }

        return driveInfo ?? throw new NotImplementedException("No unmapped drive found!");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllBytes))]
    public void WriteAllBytes_PreviousFile_ShouldOverwriteFileWithBytes(
        string path, byte[] contents)
    {
        FileSystem.File.WriteAllBytes(path, Encoding.UTF8.GetBytes("foo"));

        FileSystem.File.WriteAllBytes(path, contents);

        byte[] result = FileSystem.File.ReadAllBytes(path);
        result.Should().BeEquivalentTo(contents);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllBytes))]
    public void WriteAllBytes_ShouldCreateFileWithBytes(string path, byte[] contents)
    {
        FileSystem.File.WriteAllBytes(path, contents);

        byte[] result = FileSystem.File.ReadAllBytes(path);
        result.Should().BeEquivalentTo(contents);
    }
}
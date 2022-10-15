using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Delete))]
	public void Delete_MissingDirectory_ShouldDeleteDirectory(string path)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();

		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.Message.Should()
		   .Be($"Could not find a part of the path '{sut.FullName}'.");
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Delete))]
	public void Delete_Recursive_WithSubdirectory_ShouldDeleteDirectoryWithContent(
		string path, string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();

		sut.Delete(true);

#if NETFRAMEWORK
		// The DirectoryInfo is not updated in .NET Framework!
		sut.Exists.Should().BeTrue();
#else
		sut.Exists.Should().BeFalse();
#endif
		FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
		FileSystem.Directory.Exists(subdirectoryPath).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Delete))]
	public void Delete_ShouldDeleteDirectory(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();

		sut.Delete();

#if NETFRAMEWORK
		// The DirectoryInfo is not updated in .NET Framework!
		sut.Exists.Should().BeTrue();
#else
		sut.Exists.Should().BeFalse();
#endif
		FileSystem.Directory.Exists(sut.FullName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.DirectoryInfo(nameof(IFileSystem.IDirectoryInfo.Delete))]
	public void Delete_WithSubdirectory_ShouldNotDeleteDirectory(
		string path, string subdirectory)
	{
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();

		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
		});

		exception.Should().BeOfType<IOException>();
#if !NETFRAMEWORK
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			// Path information only included in exception message on Windows and not in .NET Framework
			exception.Should().BeOfType<IOException>()
			   .Which.Message.Should().Contain($"'{sut.FullName}'");
		}
#endif

		sut.Exists.Should().BeTrue();
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
	}
}
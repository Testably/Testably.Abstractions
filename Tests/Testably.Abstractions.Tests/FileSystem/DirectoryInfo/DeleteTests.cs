using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DeleteTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Delete_MissingDirectory_ShouldDeleteDirectory(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();

		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
			.Which.HResult.Should().Be(-2147024893);
		exception.Should().BeOfType<DirectoryNotFoundException>()
			.Which.Message.Should()
			.Be($"Could not find a part of the path '{sut.FullName}'.");
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_Recursive_WithOpenFile_ShouldThrowIOException(
		string path, string filename)
	{
		FileSystem.Initialize()
			.WithSubdirectory(path);
		string filePath = FileSystem.Path.Combine(path, filename);
		FileSystemStream openFile = FileSystem.File.OpenWrite(filePath);
		openFile.Write(new byte[]
		{
			0
		}, 0, 1);
		openFile.Flush();
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		Exception? exception = Record.Exception(() =>
		{
			sut.Delete(true);
			openFile.Write(new byte[]
			{
				0
			}, 0, 1);
			openFile.Flush();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(-2147024864);
			exception.Should().BeOfType<IOException>()
				.Which.Message.Should()
				.Contain($"{filename}'");
			FileSystem.File.Exists(filePath).Should().BeTrue();
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.File.Exists(filePath).Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_Recursive_WithSubdirectory_ShouldDeleteDirectoryWithContent(
		string path, string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
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
	public void Delete_ShouldDeleteDirectory(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
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
	public void Delete_WithSubdirectory_ShouldNotDeleteDirectory(
		string path, string subdirectory)
	{
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeTrue();

		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(-2147024751);
		}
		else if (Test.RunsOnMac)
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(66);
		}
		else
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(39);
		}

		exception.Should().BeOfType<IOException>();
#if !NETFRAMEWORK
		if (Test.RunsOnWindows)
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
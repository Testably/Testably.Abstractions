using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DeleteTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Delete_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName)
	{
		string filePath = FileSystem.Path.Combine(missingDirectory, fileName);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(filePath).Delete();
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_MissingFile_ShouldDoNothing(
		string fileName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(fileName).Delete();
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_ShouldRefreshExistsCache_ExceptOnNetFramework(string path)
	{
		FileSystem.File.WriteAllText(path, "some content");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		sut.Exists.Should().BeTrue();

		sut.Delete();

		if (Test.IsNetFramework)
		{
			sut.Exists.Should().BeTrue();
		}
		else
		{
			sut.Exists.Should().BeFalse();
		}

		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_WithOpenFile_ShouldThrowIOException_OnWindows(string filename)
	{
		FileSystem.Initialize();
		FileSystemStream openFile = FileSystem.File.OpenWrite(filename);
		openFile.Write(new byte[]
		{
			0
		}, 0, 1);
		openFile.Flush();
		IFileInfo sut = FileSystem.FileInfo.New(filename);
		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
			openFile.Write(new byte[]
			{
				0
			}, 0, 1);
			openFile.Flush();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(
				messageContains: $"{filename}'",
				hResult: -2147024864);
			FileSystem.File.Exists(filename).Should().BeTrue();
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.File.Exists(filename).Should().BeFalse();
		}
	}
}

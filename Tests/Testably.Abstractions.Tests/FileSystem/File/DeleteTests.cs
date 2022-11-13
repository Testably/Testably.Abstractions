using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

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
			FileSystem.File.Delete(filePath);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
			.Which.HResult.Should().Be(-2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_MissingFile_ShouldDoNothing(
		string fileName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(fileName);
		});

		exception.Should().BeNull();
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
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(filename);
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
			FileSystem.File.Exists(filename).Should().BeTrue();
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.File.Exists(filename).Should().BeFalse();
		}
	}
}
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class DeleteTests
{
	[Theory]
	[AutoData]
	public async Task Delete_MissingDirectory_ShouldThrowDirectoryNotFoundException(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsFalse();

		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
		});

		exception.Should().BeException<DirectoryNotFoundException>($"'{sut.FullName}'",
			hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Delete_Recursive_WithOpenFile_ShouldThrowIOException_OnWindows(
		string path, string filename)
	{
		FileSystem.Initialize()
			.WithSubdirectory(path);
		string filePath = FileSystem.Path.Combine(path, filename);
		FileSystemStream openFile = FileSystem.File.OpenWrite(filePath);
		openFile.Write([0], 0, 1);
		openFile.Flush();
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		Exception? exception = Record.Exception(() =>
		{
			sut.Delete(true);
			openFile.Write([0], 0, 1);
			openFile.Flush();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>($"{filename}'",
				hResult: -2147024864);
			await That(FileSystem.File.Exists(filePath)).IsTrue();
		}
		else
		{
			await That(exception).IsNull();
			await That(FileSystem.File.Exists(filePath)).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task Delete_Recursive_WithSubdirectory_ShouldDeleteDirectoryWithContent(
		string path, string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.Delete(true);

#if NETFRAMEWORK
		// The DirectoryInfo is not updated in .NET Framework!
		await That(sut.Exists).IsTrue();
#else
		await That(sut.Exists).IsFalse();
#endif
		await That(FileSystem.Directory.Exists(sut.FullName)).IsFalse();
		await That(FileSystem.Directory.Exists(subdirectoryPath)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Delete_ShouldDeleteDirectory(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.Delete();

#if NETFRAMEWORK
		// The DirectoryInfo is not updated in .NET Framework!
		await That(sut.Exists).IsTrue();
#else
		await That(sut.Exists).IsFalse();
#endif
		await That(FileSystem.Directory.Exists(sut.FullName)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Delete_WithSubdirectory_ShouldThrowIOException_AndNotDeleteDirectory(
		string path, string subdirectory)
	{
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
		});

		exception.Should().BeException<IOException>(
			hResult: Test.DependsOnOS(windows: -2147024751, macOS: 66, linux: 39),
			// Path information only included in exception message on Windows and not in .NET Framework
			messageContains: !Test.RunsOnWindows || Test.IsNetFramework
				? null
				: $"'{sut.FullName}'");

		await That(sut.Exists).IsTrue();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsTrue();
	}
}

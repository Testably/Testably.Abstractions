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

		void Act()
		{
			sut.Delete();
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{sut.FullName}'").And
			.WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Delete_ReadonlyDirectory_ShouldThrowIOExceptionOnWindows(string path)
	{
		IDirectoryInfo sut = FileSystem.Directory.CreateDirectory(path);
		sut.Attributes = FileAttributes.ReadOnly;
		sut.Refresh();

		void Act()
		{
			sut.Delete();
		}

		await That(Act).Throws<IOException>()
			.OnlyIf(Test.RunsOnWindows)
			.WithMessage(Test.IsNetFramework
				? $"Access to the path '{path}' is denied."
				: $"Access to the path '{sut.FullName}' is denied.").And
			.WithHResult(-2146232800);
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

		void Act()
		{
			sut.Delete(true);
			openFile.Write([0], 0, 1);
			openFile.Flush();
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"{filename}'").And
				.WithHResult(-2147024864);
			await That(FileSystem.File.Exists(filePath)).IsTrue();
		}
		else
		{
			await That(Act).DoesNotThrow();
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

		void Act()
		{
			sut.Delete();
		}

		await That(Act).Throws<IOException>()
			.WithHResult(Test.DependsOnOS(windows: -2147024751, macOS: 66, linux: 39)).And
			.WithMessageContaining(
				// Path information only included in exception message on Windows and not in .NET Framework
				!Test.RunsOnWindows || Test.IsNetFramework
					? null
					: $"'{sut.FullName}'");

		await That(sut.Exists).IsTrue();
		await That(FileSystem.Directory.Exists(sut.FullName)).IsTrue();
	}

	[Theory]
	[AutoData]
	[InlineData(null)]
	public async Task Delete_CurrentDirectory_ShouldThrowIOException_OnWindows(string? nested)
	{
		// Arrange
		string directory = FileSystem.Directory.GetCurrentDirectory();

		if (nested != null)
		{
			string nestedDirectory = FileSystem.Path.Combine(directory, nested);
			FileSystem.Directory.CreateDirectory(nestedDirectory);
			FileSystem.Directory.SetCurrentDirectory(nestedDirectory);
		}

		// Act
		void Act()
		{
			FileSystem.DirectoryInfo.New(directory).Delete(true);
		}

		// Assert
		if (Test.RunsOnWindows)
		{
			await That(Act).ThrowsExactly<IOException>().Which.HasMessage(
				$"The process cannot access the file '{directory}' because it is being used by another process."
			);
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}
}

using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class LengthTests
{
	[Theory]
	[AutoData]
	public async Task Length_MissingDirectory_ShouldThrowFileNotFoundException(
		string missingDirectory, string fileName)
	{
		string path = FileSystem.Path.Combine(missingDirectory, fileName);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			_ = sut.Length;
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessageContaining(Test.IsNetFramework
				? $"'{path}'"
				: $"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[Theory]
	[AutoData]
	public async Task Length_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			_ = sut.Length;
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessageContaining(Test.IsNetFramework
				? $"'{path}'"
				: $"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[Theory]
	[AutoData]
	public async Task Length_PathIsDirectory_ShouldThrowFileNotFoundException(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			_ = sut.Length;
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessageContaining(Test.IsNetFramework
				? $"'{path}'"
				: $"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[Theory]
	[AutoData]
	public async Task Length_WhenFileExists_ShouldBeSetCorrectly(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		long result = sut.Length;

		await That(result).IsEqualTo(bytes.Length);
	}

	[Theory]
	[AutoData]
	public async Task Length_WhenFileIsCreated_ShouldBeSetCorrectly(string path, byte[] bytes)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		FileSystem.File.WriteAllBytes(path, bytes);

		long result = sut.Length;

		await That(result).IsEqualTo(bytes.Length);
	}

	[Theory]
	[AutoData]
	public async Task Length_WhenFileIsCreatedAfterAccessed_ShouldBeSetCorrectly(
		string path, byte[] bytes)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			_ = sut.OpenRead();
		}

		FileSystem.File.WriteAllBytes(path, bytes);

		long result = sut.Length;

		await That(Act).DoesNotThrow();
		await That(result).IsEqualTo(bytes.Length);
	}

	[Theory]
	[AutoData]
	public async Task
		Length_WhenFileIsCreatedAfterLengthAccessed_ShouldThrowFileNotFoundExceptionAgain(
			string path, byte[] bytes)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			_ = sut.Length;
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining(Test.IsNetFramework
				? $"'{path}'"
				: $"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);

		FileSystem.File.WriteAllBytes(path, bytes);

		void Act2()
		{
			_ = sut.Length;
		}

		await That(Act2).Throws<FileNotFoundException>()
			.WithMessageContaining(Test.IsNetFramework
				? $"'{path}'"
				: $"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}
}

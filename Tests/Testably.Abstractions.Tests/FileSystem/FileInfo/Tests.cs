using System.IO;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public async Task Attributes_WhenFileIsMissing_SetterShouldThrowFileNotFoundException(
		string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		void Act()
		{
			sut.Attributes = FileAttributes.ReadOnly;
		}

		await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task Attributes_WhenFileIsMissing_ShouldReturnMinusOne(string path)
	{
		FileAttributes expected = (FileAttributes)(-1);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		await That(sut.Attributes).IsEqualTo(expected);
	}

	[Fact]
	public async Task Directory_ShouldReturnParentDirectory()
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());
		IFileInfo? file = initialized[1] as IFileInfo;

		await That(file?.Directory).IsNotNull();
		await That(file!.Directory!.FullName).IsEqualTo(initialized[0].FullName);
	}

	[Fact]
	public async Task DirectoryName_ShouldReturnNameOfParentDirectory()
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());
		IFileInfo? file = initialized[1] as IFileInfo;

		await That(file).IsNotNull();
		await That(file!.DirectoryName).IsEqualTo(initialized[0].FullName);
	}

	[Theory]
	[InlineData("foo", "")]
	[InlineData("foo.txt", ".txt")]
	[InlineData("foo.bar.txt", ".txt")]
	public async Task Extension_ShouldReturnExpectedValue(string fileName, string expectedValue)
	{
		IFileInfo sut = FileSystem.FileInfo.New(fileName);

		await That(sut.Extension).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task Extension_WithTrailingDot_ShouldReturnExpectedValue()
	{
		IFileInfo sut = FileSystem.FileInfo.New("foo.");

		if (Test.RunsOnWindows)
		{
			await That(sut.Extension).IsEqualTo("");
		}
		else
		{
			await That(sut.Extension).IsEqualTo(".");
		}
	}

	[Theory]
	[AutoData]
	public async Task IsReadOnly_MissingFile_ShouldBeTrue(string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		await That(fileInfo.IsReadOnly).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task IsReadOnly_SetToFalse_ShouldRemoveReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.Attributes = FileAttributes.ReadOnly;

		fileInfo.IsReadOnly = false;

		await That(fileInfo.IsReadOnly).IsFalse();
		await That(fileInfo.Attributes).IsEqualTo(FileAttributes.Normal);
	}

	[Theory]
	[AutoData]
	public async Task IsReadOnly_SetToTrue_ShouldAddReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly = true;

		await That(fileInfo.IsReadOnly).IsTrue();
		await That(fileInfo.Attributes).HasFlag(FileAttributes.ReadOnly);

		fileInfo.IsReadOnly = true;

		await That(fileInfo.IsReadOnly).IsTrue();
		await That(fileInfo.Attributes).HasFlag(FileAttributes.ReadOnly);

		fileInfo.IsReadOnly = false;

		await That(fileInfo.IsReadOnly).IsFalse();
		await That(fileInfo.Attributes).DoesNotHaveFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[AutoData]
	public async Task IsReadOnly_ShouldChangeWhenSettingReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.Attributes = FileAttributes.ReadOnly | FileAttributes.Encrypted;

		await That(fileInfo.IsReadOnly).IsTrue();
		await That(fileInfo.Attributes).HasFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[AutoData]
	public async Task IsReadOnly_ShouldInitializeToReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		await That(fileInfo.IsReadOnly).IsFalse();
		await That(fileInfo.Attributes).DoesNotHaveFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[InlineData("/foo")]
	[InlineData("./foo")]
	[InlineData("foo")]
	public async Task ToString_ShouldReturnProvidedPath(string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		string? result = fileInfo.ToString();

		await That(result).IsEqualTo(path);
	}
}

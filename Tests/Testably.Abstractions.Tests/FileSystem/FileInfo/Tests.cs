using System.IO;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Attributes_WhenFileIsMissing_SetterShouldThrowFileNotFoundException(
		string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			sut.Attributes = FileAttributes.ReadOnly;
		});

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public void Attributes_WhenFileIsMissing_ShouldReturnMinusOne(string path)
	{
		FileAttributes expected = (FileAttributes)(-1);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Attributes.Should().Be(expected);
	}

	[SkippableFact]
	public void Directory_ShouldReturnParentDirectory()
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());
		IFileInfo? file = initialized[1] as IFileInfo;

		file?.Directory.Should().NotBeNull();
		file!.Directory!.FullName.Should().Be(initialized[0].FullName);
	}

	[SkippableFact]
	public void DirectoryName_ShouldReturnNameOfParentDirectory()
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
				.WithASubdirectory().Initialized(s => s
					.WithAFile());
		IFileInfo? file = initialized[1] as IFileInfo;

		file?.Should().NotBeNull();
		file!.DirectoryName.Should().Be(initialized[0].FullName);
	}

	[SkippableTheory]
	[InlineData("foo", "")]
	[InlineData("foo.txt", ".txt")]
	[InlineData("foo.bar.txt", ".txt")]
	public void Extension_ShouldReturnExpectedValue(string fileName, string expectedValue)
	{
		IFileInfo sut = FileSystem.FileInfo.New(fileName);

		sut.Extension.Should().Be(expectedValue);
	}

	[SkippableFact]
	public void Extension_WithTrailingDot_ShouldReturnExpectedValue()
	{
		IFileInfo sut = FileSystem.FileInfo.New("foo.");

		if (Test.RunsOnWindows)
		{
			sut.Extension.Should().Be("");
		}
		else
		{
			sut.Extension.Should().Be(".");
		}
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_MissingFile_ShouldBeTrue(string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_SetToFalse_ShouldRemoveReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		fileInfo.Attributes = FileAttributes.ReadOnly;

		fileInfo.IsReadOnly = false;

		fileInfo.IsReadOnly.Should().BeFalse();
		fileInfo.Attributes.Should().Be(FileAttributes.Normal);
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_SetToTrue_ShouldAddReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly = true;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.Should().HaveFlag(FileAttributes.ReadOnly);

		fileInfo.IsReadOnly = true;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.Should().HaveFlag(FileAttributes.ReadOnly);

		fileInfo.IsReadOnly = false;

		fileInfo.IsReadOnly.Should().BeFalse();
		fileInfo.Attributes.Should().NotHaveFlag(FileAttributes.ReadOnly);
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_ShouldChangeWhenSettingReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.Attributes = FileAttributes.ReadOnly | FileAttributes.Encrypted;

		fileInfo.IsReadOnly.Should().BeTrue();
		fileInfo.Attributes.Should().HaveFlag(FileAttributes.ReadOnly);
	}

	[SkippableTheory]
	[AutoData]
	public void IsReadOnly_ShouldInitializeToReadOnlyAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		fileInfo.IsReadOnly.Should().BeFalse();
		fileInfo.Attributes.Should().NotHaveFlag(FileAttributes.ReadOnly);
	}

	[SkippableTheory]
	[InlineData("/foo")]
	[InlineData("./foo")]
	[InlineData("foo")]
	public void ToString_ShouldReturnProvidedPath(string path)
	{
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);

		string? result = fileInfo.ToString();

		result.Should().Be(path);
	}
}

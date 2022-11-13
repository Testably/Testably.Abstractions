using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineData(259)]
	[InlineData(260)]
	public void New_PathTooLong_ShouldThrowPathTooLongExceptionOnNetFramework(
		int maxLength)
	{
		string rootDrive = FileTestHelper.RootDrive();
		string path = new('a', maxLength - rootDrive.Length);
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.FileInfo.New(rootDrive + path);
		});

		if (Test.IsNetFramework)
		{
			exception.Should().BeOfType<PathTooLongException>()
			   .Which.HResult.Should().Be(-2147024690);
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void New_ShouldCreateNewFileInfoFromPath(string path)
	{
		IFileInfo result = FileSystem.FileInfo.New(path);

		result.ToString().Should().Be(path);
		result.Exists.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void New_ShouldOpenWithExistingContent(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		IFileInfo sut = FileSystem.FileInfo.New(path);

		using StreamReader streamReader = new(sut.OpenRead());
		string result = streamReader.ReadToEnd();
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void New_ShouldSetLength(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		FileSystemStream stream = FileSystem.File.Open(path, FileMode.Open,
			FileAccess.Read, FileShare.Write);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		long result = sut.Length;

		stream.Dispose();

		result.Should().Be(bytes.Length);
	}

	[SkippableFact]
	public void Wrap_Null_ShouldReturnNull()
	{
		IFileInfo? result = FileSystem.FileInfo.Wrap(null);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Wrap_ShouldWrapFromFileInfo(string path)
	{
		System.IO.FileInfo fileInfo = new(path);

		IFileInfo result = FileSystem.FileInfo.Wrap(fileInfo);

		result.FullName.Should().Be(fileInfo.FullName);
		result.Exists.Should().Be(fileInfo.Exists);
	}
}
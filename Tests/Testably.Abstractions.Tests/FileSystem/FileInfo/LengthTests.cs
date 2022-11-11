using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class LengthTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Length_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = sut.Length;
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
#if NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{path}'");
#else
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
#endif
	}

	[SkippableTheory]
	[AutoData]
	public void Length_MissingDirectory_ShouldThrowFileNotFoundException(
		string missingDirectory, string fileName)
	{
		string path = FileSystem.Path.Combine(missingDirectory, fileName);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = sut.Length;
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
#if NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{path}'");
#else
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
#endif
	}

	[SkippableTheory]
	[AutoData]
	public void Length_PathIsDirectory_ShouldThrowFileNotFoundException(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = sut.Length;
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
#if NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{path}'");
#else
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
#endif
	}

	[SkippableTheory]
	[AutoData]
	public void Length_WhenFileExists_ShouldBeSetCorrectly(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		long result = sut.Length;

		result.Should().Be(bytes.Length);
	}

	[SkippableTheory]
	[AutoData]
	public void Length_WhenFileIsCreated_ShouldBeSetCorrectly(string path, byte[] bytes)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		FileSystem.File.WriteAllBytes(path, bytes);

		long result = sut.Length;

		result.Should().Be(bytes.Length);
	}

	[SkippableTheory]
	[AutoData]
	public void Length_WhenFileIsCreatedAfterAccessed_ShouldBeSetCorrectly(
		string path, byte[] bytes)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = sut.OpenRead();
		});

		FileSystem.File.WriteAllBytes(path, bytes);

		long result = sut.Length;

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
		result.Should().Be(bytes.Length);
	}

	[SkippableTheory]
	[AutoData]
	public void
		Length_WhenFileIsCreatedAfterLengthAccessed_ShouldThrowFileNotFoundExceptionAgain(
			string path, byte[] bytes)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		Exception? exception = Record.Exception(() =>
		{
			_ = sut.Length;
		});

		FileSystem.File.WriteAllBytes(path, bytes);

		Exception? exception2 = Record.Exception(() =>
		{
			_ = sut.Length;
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
		exception2.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
#if NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{path}'");
		exception2.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{path}'");
#else
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
		exception2.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
#endif
	}
}
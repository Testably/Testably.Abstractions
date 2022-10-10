using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CopyTo))]
	public void CopyTo_DestinationExists_ShouldThrowIOExceptionAndNotCopyFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.CopyTo(destinationName);
		});

		exception.Should().BeOfType<IOException>();
		sut.Exists.Should().BeTrue();
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CopyTo))]
	public void CopyTo_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileSystem.IFileInfo result = sut.CopyTo(destinationName, true);

		sut.Exists.Should().BeTrue();
		sut.FullName.Should().Be(FileSystem.Path.GetFullPath(sourceName));
		result.Exists.Should().BeTrue();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
	}
#endif

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CopyTo))]
	public void CopyTo_ReadOnly_ShouldCopyFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		sut.IsReadOnly = true;

		sut.CopyTo(destinationName);

		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.GetAttributes(destinationName)
		   .Should().HaveFlag(FileAttributes.ReadOnly);
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CopyTo))]
	public void CopyTo_ShouldCopyFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText(sourceName, contents);
		IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(1000);

		IFileSystem.IFileInfo result = sut.CopyTo(destinationName);

		sut.FullName.Should().Be(FileSystem.Path.GetFullPath(sourceName));
		sut.Exists.Should().BeTrue();
		result.Exists.Should().BeTrue();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		result.CreationTime.Should().NotBe(sut.CreationTime);
#if !NETFRAMEWORK
		result.LastAccessTime.Should().Be(sut.LastAccessTime);
		result.LastWriteTime.Should().Be(sut.LastWriteTime);
#endif
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(contents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CopyTo))]
	public void CopyTo_SourceIsDirectory_ShouldThrowIOExceptionAndNotCopyFile(
		string sourceName,
		string destinationName)
	{
		FileSystem.Directory.CreateDirectory(sourceName);
		IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.CopyTo(destinationName);
		});

		exception.Should().BeOfType<UnauthorizedAccessException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
		FileSystem.Directory.Exists(sourceName).Should().BeTrue();
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CopyTo))]
	public void CopyTo_SourceLocked_ShouldThrowIOException(
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, FileShare.None);
		IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.CopyTo(destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>();
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
		else
		{
			FileSystem.File.Exists(sourceName).Should().BeTrue();
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.CopyTo))]
	public void CopyTo_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.CopyTo(destinationName);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>();
#else
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
#endif
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}
}
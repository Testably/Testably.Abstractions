using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class MoveToTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void MoveTo_DestinationExists_ShouldThrowIOExceptionAndNotMoveFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
			   .Which.HResult.Should().Be(-2147024713);
		}
		else
		{
			exception.Should().BeOfType<IOException>()
			   .Which.HResult.Should().Be(17);
		}

		sut.Exists.Should().BeTrue();
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void MoveTo_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		sut.MoveTo(destinationName, true);

		sut.Exists.Should().BeTrue();
		sut.ToString().Should().Be(destinationName);
		sut.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void MoveTo_Itself_ShouldDoNothing(
		string sourceName,
		string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(sourceName);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_Itself_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName)
	{
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(sourceName);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
#if !NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
#endif
		FileSystem.File.Exists(sourceName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void
		MoveTo_MissingDestinationDirectory_ShouldThrowDirectoryNotFoundExceptionAndNotMoveFile(
			string sourceName,
			string missingDirectory,
			string destinationName,
			string sourceContents)
	{
		string destinationPath =
			FileSystem.Path.Combine(missingDirectory, destinationName);
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(destinationPath);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.HResult.Should().Be(-2147024893);

		sut.Exists.Should().BeTrue();
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_ReadOnly_ShouldMoveFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		sut.IsReadOnly = true;

		sut.MoveTo(destinationName);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.GetAttributes(destinationName)
		   .Should().HaveFlag(FileAttributes.ReadOnly);
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_ShouldAddArchiveAttributeOnWindows(
		string sourceName,
		string destinationName,
		string contents,
		FileAttributes fileAttributes)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.SetAttributes(sourceName, fileAttributes);
		FileAttributes expectedAttributes = FileSystem.File.GetAttributes(sourceName);
		if (Test.RunsOnWindows)
		{
			expectedAttributes |= FileAttributes.Archive;
		}

		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		sut.MoveTo(destinationName);

		FileSystem.File.GetAttributes(destinationName)
		   .Should().Be(expectedAttributes);
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_ShouldKeepMetadata(
		string sourceName,
		string destinationName,
		string contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText(sourceName, contents);
		DateTime sourceCreationTime = FileSystem.File.GetCreationTime(sourceName);
		DateTime sourceLastAccessTime = FileSystem.File.GetLastAccessTime(sourceName);
		DateTime sourceLastWriteTime = FileSystem.File.GetLastWriteTime(sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(1000);

		sut.MoveTo(destinationName);

		FileSystem.File.GetCreationTime(destinationName)
		   .Should().Be(sourceCreationTime);
		FileSystem.File.GetLastAccessTime(destinationName)
		   .Should().Be(sourceLastAccessTime);
		FileSystem.File.GetLastWriteTime(destinationName)
		   .Should().Be(sourceLastWriteTime);
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_ShouldMoveFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		sut.MoveTo(destinationName);

		sut.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		sut.Exists.Should().BeTrue();
		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_ShouldNotAdjustTimes(string source, string destination)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText(source, "foo");
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		IFileInfo sut = FileSystem.FileInfo.New(source);
		DateTime expectedCreationTime = sut.CreationTime;
		DateTime expectedLastAccessTime = sut.LastAccessTime;
		DateTime expectedLastWriteTime = sut.LastWriteTime;

		sut.MoveTo(destination);

		DateTime creationTime = FileSystem.File.GetCreationTime(destination);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTime(destination);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTime(destination);

		sut.CreationTime.Should().Be(expectedCreationTime);
		sut.LastAccessTime.Should().Be(expectedLastAccessTime);
		sut.LastWriteTime.Should().Be(expectedLastWriteTime);
		creationTime.Should().Be(expectedCreationTime);
		lastAccessTime.Should().Be(expectedLastAccessTime);
		lastWriteTime.Should().Be(expectedLastWriteTime);
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_SourceLocked_ShouldThrowIOException(
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, FileShare.Read);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
			   .Which.HResult.Should().Be(-2147024864);
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
		else
		{
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(destinationName);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
#if !NETFRAMEWORK
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
#endif
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}
}
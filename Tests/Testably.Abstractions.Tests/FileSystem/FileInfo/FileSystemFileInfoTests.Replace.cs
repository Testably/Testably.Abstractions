using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Replace_DestinationIsDirectory_ShouldThrowUnauthorizedAccessException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		FileSystem.Directory.CreateDirectory(destinationName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		exception.Should().BeOfType<UnauthorizedAccessException>();
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_DestinationMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		exception.Should().BeOfType<FileNotFoundException>();
		FileSystem.File.Exists(backupName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_ReadOnly_WithIgnoreMetadataError_ShouldReplaceFile(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		sut.IsReadOnly = true;

		IFileInfo result = sut.Replace(destinationName, backupName, true);

		sut.Exists.Should().BeFalse();
		FileSystem.File.Exists(sourceName).Should().BeFalse();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
		FileSystem.File.GetAttributes(destinationName)
		   .Should().HaveFlag(FileAttributes.ReadOnly);
		FileSystem.File.Exists(backupName).Should().BeTrue();
		FileSystem.File.ReadAllText(backupName).Should().Be(destinationContents);
	}

	[SkippableTheory]
	[AutoData]
	public void
		Replace_ReadOnly_WithoutIgnoreMetadataError_ShouldThrowUnauthorizedAccessException(
			string sourceName,
			string destinationName,
			string backupName,
			string sourceContents,
			string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		sut.IsReadOnly = true;

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<UnauthorizedAccessException>();
			FileSystem.File.Exists(sourceName).Should().BeTrue();
			FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
			FileSystem.File.GetAttributes(destinationName)
			   .Should().NotHaveFlag(FileAttributes.ReadOnly);
			FileSystem.File.Exists(backupName).Should().BeFalse();
		}
		else
		{
			exception.Should().BeNull();
			sut.Exists.Should().BeFalse();
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
			FileSystem.File.Exists(backupName).Should().BeTrue();
			FileSystem.File.ReadAllText(backupName).Should().Be(destinationContents);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_ShouldAddArchiveAttributeOnWindows(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents,
		FileAttributes sourceFileAttributes,
		FileAttributes destinationFileAttributes)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.SetAttributes(sourceName, sourceFileAttributes);
		FileAttributes expectedSourceAttributes =
			FileSystem.File.GetAttributes(sourceName);
		if (Test.RunsOnWindows)
		{
			expectedSourceAttributes |= FileAttributes.Archive;
		}

		FileSystem.File.WriteAllText(destinationName, destinationContents);
		FileSystem.File.SetAttributes(destinationName, destinationFileAttributes);
		FileAttributes expectedDestinationAttributes =
			FileSystem.File.GetAttributes(destinationName);
		if (Test.RunsOnWindows)
		{
			expectedDestinationAttributes |= FileAttributes.Archive;
		}

		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		sut.Replace(destinationName, backupName, true);

		FileSystem.File.GetAttributes(destinationName)
		   .Should().Be(expectedSourceAttributes);
		FileSystem.File.GetAttributes(backupName)
		   .Should().Be(expectedDestinationAttributes);
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_ShouldKeepMetadata(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText(sourceName, sourceContents);
		DateTime sourceLastAccessTime = FileSystem.File.GetLastAccessTime(sourceName);
		DateTime sourceLastWriteTime = FileSystem.File.GetLastWriteTime(sourceName);
		TimeSystem.Thread.Sleep(1000);

		FileSystem.File.WriteAllText(destinationName, destinationContents);
		DateTime destinationCreationTime =
			FileSystem.File.GetCreationTime(destinationName);
		DateTime destinationLastAccessTime =
			FileSystem.File.GetLastAccessTime(destinationName);
		DateTime destinationLastWriteTime =
			FileSystem.File.GetLastWriteTime(destinationName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		TimeSystem.Thread.Sleep(1000);

		sut.Replace(destinationName, backupName);

		FileSystem.File.GetCreationTime(destinationName)
		   .Should().Be(destinationCreationTime);
		FileSystem.File.GetLastAccessTime(destinationName)
		   .Should().Be(sourceLastAccessTime);
		FileSystem.File.GetLastWriteTime(destinationName)
		   .Should().Be(sourceLastWriteTime);
		FileSystem.File.GetCreationTime(backupName)
		   .Should().Be(destinationCreationTime);
		FileSystem.File.GetLastAccessTime(backupName)
		   .Should().Be(destinationLastAccessTime);
		FileSystem.File.GetLastWriteTime(backupName)
		   .Should().Be(destinationLastWriteTime);
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_ShouldReplaceFile(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileInfo result = sut.Replace(destinationName, backupName);

		sut.Exists.Should().BeFalse();
		FileSystem.File.Exists(sourceName).Should().BeFalse();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
		FileSystem.File.Exists(backupName).Should().BeTrue();
		FileSystem.File.ReadAllText(backupName).Should().Be(destinationContents);
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_SourceIsDirectory_ShouldThrowUnauthorizedAccessException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Tests sometimes throw IOException on Linux");

		FileSystem.Directory.CreateDirectory(sourceName);
		FileSystem.File.WriteAllText(destinationName, null);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		exception.Should().BeOfType<UnauthorizedAccessException>();
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_SourceLocked_ShouldThrowIOException(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, FileShare.Read);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		stream.Dispose();
		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>();
			sut.Exists.Should().BeTrue();
			FileSystem.File.Exists(sourceName).Should().BeTrue();
			FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
			FileSystem.File.GetAttributes(destinationName)
			   .Should().NotHaveFlag(FileAttributes.ReadOnly);
			FileSystem.File.Exists(backupName).Should().BeFalse();
		}
		else
		{
			sut.Exists.Should().BeFalse();
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
			FileSystem.File.Exists(backupName).Should().BeTrue();
			FileSystem.File.ReadAllText(backupName).Should().Be(destinationContents);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(destinationName, null);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		exception.Should().BeOfType<FileNotFoundException>();
		if (Test.RunsOnWindows)
		{
			// Behaviour on Linux/MacOS is uncertain
			FileSystem.File.Exists(backupName).Should().BeFalse();
		}
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_WithExistingBackupFile_ShouldIgnoreBackup(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents,
		string backupContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		FileSystem.File.WriteAllText(backupName, backupContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileInfo result = sut.Replace(destinationName, null);

		sut.Exists.Should().BeFalse();
		FileSystem.File.Exists(sourceName).Should().BeFalse();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
		FileSystem.File.Exists(backupName).Should().BeTrue();
		FileSystem.File.ReadAllText(backupName).Should().Be(backupContents);
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_WithoutBackup_ShouldReplaceFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileInfo result = sut.Replace(destinationName, null);

		sut.Exists.Should().BeFalse();
		FileSystem.File.Exists(sourceName).Should().BeFalse();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
	}
}
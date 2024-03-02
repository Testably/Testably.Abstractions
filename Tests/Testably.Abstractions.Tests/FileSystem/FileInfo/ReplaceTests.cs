using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReplaceTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Replace_BackupDirectoryMissing_ShouldThrowCorrectException(
		string sourceName,
		string destinationName,
		string missingDirectory,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, "some content");
		string backupPath = FileSystem.Path.Combine(missingDirectory, backupName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupPath);
		});

		exception.Should().BeFileOrDirectoryNotFoundException();
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_DestinationDirectoryMissing_ShouldThrowDirectoryNotFoundException(
		string sourceName,
		string missingDirectory,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, "some content");
		string destinationPath =
			FileSystem.Path.Combine(missingDirectory, destinationName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationPath, backupName);
		});

		exception.Should().BeFileOrDirectoryNotFoundException();
	}

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

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
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

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		FileSystem.Should().NotHaveFile(backupName);
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

		sut.Should().NotExist();
		FileSystem.Should().NotHaveFile(sourceName);
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(sourceContents)
			.And.HasAttribute(FileAttributes.ReadOnly);
		FileSystem.Should().HaveFile(backupName)
			.Which.HasContent(destinationContents);
	}

	[SkippableTheory]
	[AutoData]
	public void
		Replace_ReadOnly_WithoutIgnoreMetadataError_ShouldThrowUnauthorizedAccessException_OnWindows(
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
			exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
			FileSystem.Should().HaveFile(sourceName)
				.Which.HasContent(sourceContents);
			FileSystem.Should().HaveFile(destinationName)
				.Which.HasContent(destinationContents);
			FileSystem.File.GetAttributes(destinationName)
				.Should().NotHaveFlag(FileAttributes.ReadOnly);
			FileSystem.Should().NotHaveFile(backupName);
		}
		else
		{
			exception.Should().BeNull();
			sut.Should().NotExist();
			FileSystem.Should().NotHaveFile(sourceName);
			FileSystem.Should().HaveFile(destinationName)
				.Which.HasContent(sourceContents);
			FileSystem.Should().HaveFile(backupName)
				.Which.HasContent(destinationContents);
		}
	}

	[SkippableTheory]
	[InlineAutoData(FileAttributes.Hidden, FileAttributes.System)]
	[InlineAutoData(FileAttributes.System, FileAttributes.Hidden)]
	public void Replace_ShouldAddArchiveAttribute_OnWindows(
		FileAttributes sourceFileAttributes,
		FileAttributes destinationFileAttributes,
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
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

		sut.Replace(destinationName, backupName);

		if (Test.RunsOnMac)
		{
			FileSystem.File.GetAttributes(destinationName)
				.Should().Be(expectedSourceAttributes);
		}
		else
		{
			FileSystem.File.GetAttributes(destinationName)
				.Should().Be(expectedDestinationAttributes);
		}

		FileSystem.File.GetAttributes(backupName)
			.Should().Be(expectedDestinationAttributes);
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_WhenFileIsReadOnly_ShouldThrowUnauthorizedAccessException_OnWindows(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(sourceName, sourceContents);

		FileSystem.File.WriteAllText(destinationName, destinationContents);
		FileSystem.File.SetAttributes(destinationName, FileAttributes.ReadOnly);

		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
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
		DateTime sourceCreationTime = FileSystem.File.GetCreationTime(sourceName);
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

		if (Test.RunsOnWindows)
		{
			FileSystem.File.GetCreationTime(destinationName)
				.Should().Be(destinationCreationTime);
		}
		else
		{
			FileSystem.File.GetCreationTime(destinationName)
				.Should().Be(sourceCreationTime);
		}

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

		sut.Should().NotExist();
		FileSystem.Should().NotHaveFile(sourceName);
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(sourceContents);
		FileSystem.Should().HaveFile(backupName)
			.Which.HasContent(destinationContents);
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_SourceDirectoryMissing_ShouldThrowFileNotFoundException(
		string missingDirectory,
		string sourceName,
		string destinationName,
		string backupName)
	{
		string sourcePath = FileSystem.Path.Combine(missingDirectory, sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourcePath);

		Exception? exception = Record.Exception(() =>
		{
			sut.Replace(destinationName, backupName);
		});

		exception.Should().BeFileOrDirectoryNotFoundException();
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

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}

	[SkippableTheory]
	[AutoData]
	public void Replace_SourceLocked_ShouldThrowIOException_OnWindows(
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
			exception.Should().BeException<IOException>(hResult: -2147024864);
			sut.Should().Exist();
			FileSystem.Should().HaveFile(sourceName)
				.Which.HasContent(sourceContents);
			FileSystem.Should().HaveFile(destinationName)
				.Which.HasContent(destinationContents);
			FileSystem.File.GetAttributes(destinationName)
				.Should().NotHaveFlag(FileAttributes.ReadOnly);
			FileSystem.Should().NotHaveFile(backupName);
		}
		else
		{
			sut.Should().NotExist();
			FileSystem.Should().NotHaveFile(sourceName);
			FileSystem.Should().HaveFile(destinationName)
				.Which.HasContent(sourceContents);
			FileSystem.Should().HaveFile(backupName)
				.Which.HasContent(destinationContents);
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

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		if (Test.RunsOnWindows)
		{
			// Behaviour on Linux/MacOS is uncertain
			FileSystem.Should().NotHaveFile(backupName);
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

		sut.Should().NotExist();
		FileSystem.Should().NotHaveFile(sourceName);
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(sourceContents);
		FileSystem.Should().HaveFile(backupName)
			.Which.HasContent(backupContents);
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

		sut.Should().NotExist();
		FileSystem.Should().NotHaveFile(sourceName);
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(sourceContents);
	}
}

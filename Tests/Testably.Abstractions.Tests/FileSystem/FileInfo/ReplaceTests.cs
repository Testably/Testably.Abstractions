using System.IO;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class ReplaceTests
{
	[Theory]
	[AutoData]
	public async Task Replace_BackupDirectoryMissing_ShouldThrowCorrectException(
		string sourceName,
		string destinationName,
		string missingDirectory,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, "some content");
		string backupPath = FileSystem.Path.Combine(missingDirectory, backupName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.Replace(destinationName, backupPath);
		}

		await That(Act).ThrowsAFileOrDirectoryNotFoundException();
	}

	[Theory]
	[AutoData]
	public async Task Replace_DestinationDirectoryMissing_ShouldThrowDirectoryNotFoundException(
		string sourceName,
		string missingDirectory,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, "some content");
		string destinationPath =
			FileSystem.Path.Combine(missingDirectory, destinationName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.Replace(destinationPath, backupName);
		}

		await That(Act).ThrowsAFileOrDirectoryNotFoundException();
	}

	[Theory]
	[AutoData]
	public async Task Replace_DestinationIsDirectory_ShouldThrowUnauthorizedAccessException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		FileSystem.Directory.CreateDirectory(destinationName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.Replace(destinationName, backupName);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Theory]
	[AutoData]
	public async Task Replace_DestinationMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.Replace(destinationName, backupName);
		}

		await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		await That(FileSystem.File.Exists(backupName)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Replace_ReadOnly_WithIgnoreMetadataError_ShouldReplaceFile(
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

		await That(sut.Exists).IsFalse();
		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(result.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.GetAttributes(destinationName)).HasFlag(FileAttributes.ReadOnly);
		await That(FileSystem.File.Exists(backupName)).IsTrue();
		await That(FileSystem.File.ReadAllText(backupName)).IsEqualTo(destinationContents);
	}

	[Theory]
	[AutoData]
	public async Task Replace_ReadOnly_WithoutIgnoreMetadataError_ShouldThrowUnauthorizedAccessException_OnWindows(
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

		void Act()
		{
			sut.Replace(destinationName, backupName);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(destinationContents);
			await That(FileSystem.File.GetAttributes(destinationName)).DoesNotHaveFlag(FileAttributes.ReadOnly);
			await That(FileSystem.File.Exists(backupName)).IsFalse();
		}
		else
		{
			await That(Act).DoesNotThrow();
			await That(sut.Exists).IsFalse();
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
			await That(FileSystem.File.Exists(backupName)).IsTrue();
			await That(FileSystem.File.ReadAllText(backupName)).IsEqualTo(destinationContents);
		}
	}

	[Theory]
	[InlineAutoData(FileAttributes.Hidden, FileAttributes.System)]
	[InlineAutoData(FileAttributes.System, FileAttributes.Hidden)]
	public async Task Replace_ShouldAddArchiveAttribute_OnWindows(
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
			await That(FileSystem.File.GetAttributes(destinationName)).IsEqualTo(expectedSourceAttributes);
		}
		else
		{
			await That(FileSystem.File.GetAttributes(destinationName)).IsEqualTo(expectedDestinationAttributes);
		}

		await That(FileSystem.File.GetAttributes(backupName)).IsEqualTo(expectedDestinationAttributes);
	}

	[Theory]
	[AutoData]
	public async Task Replace_ShouldKeepMetadata(
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

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
			await That(FileSystem.File.GetCreationTime(destinationName)).IsEqualTo(destinationCreationTime);
		}
		else
		{
			await That(FileSystem.File.GetCreationTime(destinationName)).IsEqualTo(sourceCreationTime);
		}

		await That(FileSystem.File.GetLastAccessTime(destinationName)).IsEqualTo(sourceLastAccessTime);
		await That(FileSystem.File.GetLastWriteTime(destinationName)).IsEqualTo(sourceLastWriteTime);
		await That(FileSystem.File.GetCreationTime(backupName)).IsEqualTo(destinationCreationTime);
		await That(FileSystem.File.GetLastAccessTime(backupName)).IsEqualTo(destinationLastAccessTime);
		await That(FileSystem.File.GetLastWriteTime(backupName)).IsEqualTo(destinationLastWriteTime);
	}

	[Theory]
	[AutoData]
	public async Task Replace_ShouldReplaceFile(
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

		await That(sut.Exists).IsFalse();
		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(result.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(backupName)).IsTrue();
		await That(FileSystem.File.ReadAllText(backupName)).IsEqualTo(destinationContents);
	}

	[Theory]
	[AutoData]
	public async Task Replace_SourceDirectoryMissing_ShouldThrowFileNotFoundException(
		string missingDirectory,
		string sourceName,
		string destinationName,
		string backupName)
	{
		string sourcePath = FileSystem.Path.Combine(missingDirectory, sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourcePath);

		void Act()
		{
			sut.Replace(destinationName, backupName);
		}

		await That(Act).ThrowsAFileOrDirectoryNotFoundException();
	}

	[Theory]
	[AutoData]
	public async Task Replace_SourceIsDirectory_ShouldThrowUnauthorizedAccessException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Tests sometimes throw IOException on Linux");

		FileSystem.Directory.CreateDirectory(sourceName);
		FileSystem.File.WriteAllText(destinationName, null);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.Replace(destinationName, backupName);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Theory]
	[InlineAutoData(FileAccess.Read, FileShare.None)]
	[InlineAutoData(FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.Read, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.Read, FileShare.Write)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.None)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Write)]
	[InlineAutoData(FileAccess.Write, FileShare.None)]
	[InlineAutoData(FileAccess.Write, FileShare.Read)]
	[InlineAutoData(FileAccess.Write, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.Write, FileShare.Write)]
	public async Task Replace_SourceLocked_ShouldThrowIOException_OnWindows(
		FileAccess fileAccess,
		FileShare fileShare,
		string sourceName,
		string destinationName,
		string backupName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			using FileSystemStream _ = FileSystem.File.Open(
				sourceName, FileMode.Open, fileAccess, fileShare);
			sut.Replace(destinationName, backupName);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>().WithHResult(-2147024864);
			await That(sut.Exists).IsTrue();
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(destinationContents);
			await That(FileSystem.File.GetAttributes(destinationName)).DoesNotHaveFlag(FileAttributes.ReadOnly);
			await That(FileSystem.File.Exists(backupName)).IsFalse();
		}
		else
		{
			// https://github.com/dotnet/runtime/issues/52700
			await That(Act).DoesNotThrow();
			await That(sut.Exists).IsFalse();
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
			await That(FileSystem.File.Exists(backupName)).IsTrue();
			await That(FileSystem.File.ReadAllText(backupName)).IsEqualTo(destinationContents);
		}
	}

	[Theory]
	[AutoData]
	public async Task Replace_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(destinationName, null);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.Replace(destinationName, backupName);
		}

		await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		if (Test.RunsOnWindows)
		{
			// Behaviour on Linux/MacOS is uncertain
			await That(FileSystem.File.Exists(backupName)).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task Replace_WhenFileIsReadOnly_ShouldThrowUnauthorizedAccessException_OnWindows(
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

		void Act()
		{
			sut.Replace(destinationName, backupName);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Theory]
	[AutoData]
	public async Task Replace_WithExistingBackupFile_ShouldIgnoreBackup(
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

		await That(sut.Exists).IsFalse();
		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(result.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(backupName)).IsTrue();
		await That(FileSystem.File.ReadAllText(backupName)).IsEqualTo(backupContents);
	}

	[Theory]
	[AutoData]
	public async Task Replace_WithoutBackup_ShouldReplaceFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileInfo result = sut.Replace(destinationName, null);

		await That(sut.Exists).IsFalse();
		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(result.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
	}
}

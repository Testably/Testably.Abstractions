using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReplaceTests
{
	[Theory]
	[AutoData]
	public async Task Replace_CaseOnlyChange_ShouldThrowIOException(
		string name, string contents)
	{
		string sourceName = name.ToLowerInvariant();
		string destinationName = name.ToUpperInvariant();
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.WriteAllText(destinationName, "other-content");

		void Act()
		{
			FileSystem.File.Replace(sourceName, destinationName, null);
		}

		if (Test.RunsOnLinux)
		{
			await That(Act).DoesNotThrow();
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
		}
		else if (Test.RunsOnMac)
		{
			await That(Act).Throws<IOException>()
				.WithHResult(-2146232800).And
				.WithMessage(
					$"The source '{FileSystem.Path.GetFullPath(sourceName)}' and destination '{FileSystem.Path.GetFullPath(destinationName)}' are the same file")
				.AsPrefix();
		}
		else
		{
			await That(Act).Throws<IOException>()
				.WithHResult(-2147024864).And
				.WithMessage("The process cannot access the file").AsPrefix();
		}
	}

	[Theory]
	[AutoData]
	public async Task Replace_DestinationDirectoryDoesNotExist_ShouldThrowCorrectException(
		string source)
	{
		FileSystem.Initialize()
			.WithFile(source);
		string destination = FileTestHelper.RootDrive(Test, "not-existing/path/foo.txt");

		void Act()
		{
			FileSystem.File.Replace(source, destination, null);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
		}
		else
		{
			await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		}
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

		void Act()
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
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

		void Act()
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
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
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Replace(sourceName, destinationName, backupName, true);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEquivalentTo(sourceContents);
		await That(FileSystem.File.GetAttributes(destinationName)).HasFlag(FileAttributes.ReadOnly);
		await That(FileSystem.File.Exists(backupName)).IsTrue();
		await That(FileSystem.File.ReadAllText(backupName)).IsEquivalentTo(destinationContents);
	}

	[Theory]
	[AutoData]
	public async Task
		Replace_ReadOnly_WithoutIgnoreMetadataError_ShouldThrowUnauthorizedAccessException_OnWindows(
			string sourceName,
			string destinationName,
			string backupName,
			string sourceContents,
			string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		void Act()
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.ReadAllText(sourceName)).IsEquivalentTo(sourceContents);
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName))
				.IsEquivalentTo(destinationContents);
			await That(FileSystem.File.GetAttributes(destinationName))
				.DoesNotHaveFlag(FileAttributes.ReadOnly);
			await That(FileSystem.File.Exists(backupName)).IsFalse();
		}
		else
		{
			await That(Act).DoesNotThrow();
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName)).IsEquivalentTo(sourceContents);
			await That(FileSystem.File.Exists(backupName)).IsTrue();
			await That(FileSystem.File.ReadAllText(backupName)).IsEquivalentTo(destinationContents);
		}
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

		FileSystem.File.Replace(sourceName, destinationName, backupName);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEquivalentTo(sourceContents);
		await That(FileSystem.File.Exists(backupName)).IsTrue();
		await That(FileSystem.File.ReadAllText(backupName)).IsEquivalentTo(destinationContents);
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

		void Act()
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
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

		void Act()
		{
			using (FileSystemStream _ = FileSystem.File.Open(
				sourceName, FileMode.Open, fileAccess, fileShare))
			{
				FileSystem.File.Replace(sourceName, destinationName, backupName);
			}
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>().WithHResult(-2147024864);
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.ReadAllText(sourceName)).IsEquivalentTo(sourceContents);
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName))
				.IsEquivalentTo(destinationContents);
			await That(FileSystem.File.GetAttributes(destinationName))
				.DoesNotHaveFlag(FileAttributes.ReadOnly);
			await That(FileSystem.File.Exists(backupName)).IsFalse();
		}
		else
		{
			// https://github.com/dotnet/runtime/issues/52700
			await That(Act).DoesNotThrow();
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
			await That(FileSystem.File.ReadAllText(destinationName)).IsEquivalentTo(sourceContents);
			await That(FileSystem.File.Exists(backupName)).IsTrue();
			await That(FileSystem.File.ReadAllText(backupName)).IsEquivalentTo(destinationContents);
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

		void Act()
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
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

		FileSystem.File.Replace(sourceName, destinationName, null);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEquivalentTo(sourceContents);
		await That(FileSystem.File.Exists(backupName)).IsTrue();
		await That(FileSystem.File.ReadAllText(backupName)).IsEquivalentTo(backupContents);
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

		FileSystem.File.Replace(sourceName, destinationName, null);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEquivalentTo(sourceContents);
	}
}

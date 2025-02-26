using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReplaceTests
{
	[Theory]
	[AutoData]
	public void Replace_CaseOnlyChange_ShouldThrowIOException(
		string name, string contents)
	{
		string sourceName = name.ToLowerInvariant();
		string destinationName = name.ToUpperInvariant();
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.WriteAllText(destinationName, "other-content");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, null);
		});


		if (Test.RunsOnLinux)
		{
			exception.Should().BeNull();
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
		}
		else if (Test.RunsOnMac)
		{
			exception.Should().BeException<IOException>(
				hResult: -2146232800,
				messageContains: $"The source '{FileSystem.Path.GetFullPath(sourceName)}' and destination '{FileSystem.Path.GetFullPath(destinationName)}' are the same file");
		}
		else
		{
			exception.Should().BeException<IOException>(
				hResult:  -2147024864,
				messageContains: "The process cannot access the file");
		}
	}

	[Theory]
	[AutoData]
	public void
		Replace_DestinationDirectoryDoesNotExist_ShouldThrowCorrectException(
			string source)
	{
		FileSystem.Initialize()
			.WithFile(source);
		string destination = FileTestHelper.RootDrive(Test, "not-existing/path/foo.txt");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(source, destination, null);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
		}
		else
		{
			exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		}
	}

	[Theory]
	[AutoData]
	public void Replace_DestinationIsDirectory_ShouldThrowUnauthorizedAccessException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		FileSystem.Directory.CreateDirectory(destinationName);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}

	[Theory]
	[AutoData]
	public void Replace_DestinationMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(sourceName, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
		});

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		FileSystem.File.Exists(backupName).Should().BeFalse();
	}

	[Theory]
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
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Replace(sourceName, destinationName, backupName, true);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(sourceContents);
		FileSystem.File.GetAttributes(destinationName).Should().HaveFlag(FileAttributes.ReadOnly);
		FileSystem.File.Exists(backupName).Should().BeTrue();
		FileSystem.File.ReadAllText(backupName).Should().BeEquivalentTo(destinationContents);
	}

	[Theory]
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
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
			FileSystem.File.Exists(sourceName).Should().BeTrue();
			FileSystem.File.ReadAllText(sourceName).Should().BeEquivalentTo(sourceContents);
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(destinationContents);
			FileSystem.File.GetAttributes(destinationName)
				.Should().NotHaveFlag(FileAttributes.ReadOnly);
			FileSystem.File.Exists(backupName).Should().BeFalse();
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(sourceContents);
			FileSystem.File.Exists(backupName).Should().BeTrue();
			FileSystem.File.ReadAllText(backupName).Should().BeEquivalentTo(destinationContents);
		}
	}

	[Theory]
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

		FileSystem.File.Replace(sourceName, destinationName, backupName);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(sourceContents);
		FileSystem.File.Exists(backupName).Should().BeTrue();
		FileSystem.File.ReadAllText(backupName).Should().BeEquivalentTo(destinationContents);
	}

	[Theory]
	[AutoData]
	public void Replace_SourceIsDirectory_ShouldThrowUnauthorizedAccessException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Tests sometimes throw IOException on Linux");

		FileSystem.Directory.CreateDirectory(sourceName);
		FileSystem.File.WriteAllText(destinationName, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
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
	public void Replace_SourceLocked_ShouldThrowIOException_OnWindows(
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

		Exception? exception;
		using (FileSystemStream _ = FileSystem.File.Open(
			sourceName, FileMode.Open, fileAccess, fileShare))
		{
			exception = Record.Exception(() =>
			{
				FileSystem.File.Replace(sourceName, destinationName, backupName);
			});
		}

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(hResult: -2147024864);
			FileSystem.File.Exists(sourceName).Should().BeTrue();
			FileSystem.File.ReadAllText(sourceName).Should().BeEquivalentTo(sourceContents);
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(destinationContents);
			FileSystem.File.GetAttributes(destinationName)
				.Should().NotHaveFlag(FileAttributes.ReadOnly);
			FileSystem.File.Exists(backupName).Should().BeFalse();
		}
		else
		{
			// https://github.com/dotnet/runtime/issues/52700
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(sourceContents);
			FileSystem.File.Exists(backupName).Should().BeTrue();
			FileSystem.File.ReadAllText(backupName).Should().BeEquivalentTo(destinationContents);
		}
	}

	[Theory]
	[AutoData]
	public void Replace_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName,
		string backupName)
	{
		FileSystem.File.WriteAllText(destinationName, null);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
		});

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		if (Test.RunsOnWindows)
		{
			// Behaviour on Linux/MacOS is uncertain
			FileSystem.File.Exists(backupName).Should().BeFalse();
		}
	}

	[Theory]
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

		FileSystem.File.Replace(sourceName, destinationName, null);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(sourceContents);
		FileSystem.File.Exists(backupName).Should().BeTrue();
		FileSystem.File.ReadAllText(backupName).Should().BeEquivalentTo(backupContents);
	}

	[Theory]
	[AutoData]
	public void Replace_WithoutBackup_ShouldReplaceFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		FileSystem.File.Replace(sourceName, destinationName, null);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(sourceContents);
	}
}

using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReplaceTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		Replace_DestinationDirectoryDoesNotExist_ShouldThrowCorrectException(
			string source)
	{
		FileSystem.Initialize()
			.WithFile(source);
		string destination = FileTestHelper.RootDrive("not-existing/path/foo.txt");

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

	[SkippableTheory]
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

	[SkippableTheory]
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
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Replace(sourceName, destinationName, backupName, true);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
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
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
			FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
			FileSystem.File.Exists(backupName).Should().BeTrue();
			FileSystem.File.ReadAllText(backupName).Should().Be(destinationContents);
		}
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

		FileSystem.File.Replace(sourceName, destinationName, backupName);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
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

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
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

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Replace(sourceName, destinationName, backupName);
		});

		stream.Dispose();
		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(hResult: -2147024864);
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

		FileSystem.File.Replace(sourceName, destinationName, null);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
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

		FileSystem.File.Replace(sourceName, destinationName, null);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
	}
}

using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class CopyToTests
{
	[Theory]
	[AutoData]
	public async Task CopyTo_DestinationExists_ShouldThrowIOException_AndNotCopyFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.CopyTo(destinationName);
		}

		await That(Act).Throws<IOException>().WithHResult(Test.RunsOnWindows ? -2147024816 : 17);
		await That(sut.Exists).IsTrue();
		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[Theory]
	[AutoData]
	public async Task CopyTo_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileInfo result = sut.CopyTo(destinationName, true);

		await That(sut.Exists).IsTrue();
		await That(sut.FullName).IsEqualTo(FileSystem.Path.GetFullPath(sourceName));
		await That(result.Exists).IsTrue();
		await That(result.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
	}
#endif

	[Theory]
	[AutoData]
	public async Task CopyTo_ReadOnly_ShouldCopyFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		sut.IsReadOnly = true;

		sut.CopyTo(destinationName);

		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
		await That(FileSystem.File.GetAttributes(destinationName)).HasFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.System)]
	public async Task CopyTo_ShouldAddArchiveAttribute_OnWindows(
		FileAttributes fileAttributes,
		string sourceName,
		string destinationName,
		string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.SetAttributes(sourceName, fileAttributes);
		FileAttributes expectedAttributes = FileSystem.File.GetAttributes(sourceName);
		if (Test.RunsOnWindows)
		{
			expectedAttributes |= FileAttributes.Archive;
		}

		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileInfo result = sut.CopyTo(destinationName);

		await That(result.Attributes).IsEqualTo(expectedAttributes);
		await That(FileSystem.File.GetAttributes(destinationName)).IsEqualTo(expectedAttributes);
	}

	[Theory]
	[AutoData]
	public async Task CopyTo_ShouldCopyFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(EnsureTimeout);

		IFileInfo result = sut.CopyTo(destinationName);

		await That(sut.FullName).IsEqualTo(FileSystem.Path.GetFullPath(sourceName));
		await That(sut.Exists).IsTrue();
		await That(result.Exists).IsTrue();
		await That(result.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(contents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task CopyTo_ShouldKeepMetadata(
		string sourceName,
		string destinationName,
		string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText(sourceName, contents);
		DateTime sourceCreationTime = FileSystem.File.GetCreationTime(sourceName);
		DateTime sourceLastWriteTime = FileSystem.File.GetLastWriteTime(sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(1000);

		DateTime updatedTime = TimeSystem.DateTime.Now;
		sut.CopyTo(destinationName);

		if (Test.RunsOnWindows)
		{
			await That(FileSystem.File.GetCreationTime(destinationName))
				.IsOnOrAfter(updatedTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(FileSystem.File.GetCreationTime(destinationName))
				.IsOnOrAfter(sourceCreationTime.ApplySystemClockTolerance()).And
				.IsBefore(updatedTime);
		}

		if (Test.RunsOnMac)
		{
			await That(FileSystem.File.GetLastAccessTime(destinationName))
				.IsOnOrAfter(sourceCreationTime.ApplySystemClockTolerance()).And
				.IsBefore(updatedTime);
		}
		else
		{
			await That(FileSystem.File.GetLastAccessTime(destinationName))
				.IsOnOrAfter(updatedTime.ApplySystemClockTolerance());
		}

		await That(FileSystem.File.GetLastWriteTime(destinationName))
			.IsEqualTo(sourceLastWriteTime);
	}

	[Theory]
	[InlineAutoData(FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.Read, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.Write, FileShare.Read)]
	[InlineAutoData(FileAccess.Write, FileShare.ReadWrite)]
	public async Task CopyTo_SourceAccessedWithReadShare_ShouldNotThrow(
		FileAccess fileAccess,
		FileShare fileShare,
		string sourcePath,
		string destinationPath,
		string sourceContents)
	{
		FileSystem.Initialize().WithFile(sourcePath)
			.Which(f => f.HasStringContent(sourceContents));
		IFileInfo sut = FileSystem.FileInfo.New(sourcePath);
		using (FileSystem.FileStream
			.New(sourcePath, FileMode.Open, fileAccess, fileShare))
		{
			sut.CopyTo(destinationPath);
		}

		await That(FileSystem.File.Exists(destinationPath)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationPath)).IsEqualTo(sourceContents);
	}

	[Theory]
	[InlineAutoData(FileAccess.Read)]
	[InlineAutoData(FileAccess.ReadWrite)]
	[InlineAutoData(FileAccess.Write)]
	public async Task CopyTo_SourceAccessedWithWriteShare_ShouldNotThrowOnLinuxOrMac(
		FileAccess fileAccess,
		string sourcePath,
		string destinationPath,
		string sourceContents)
	{
		Skip.If(Test.RunsOnWindows, "see https://github.com/dotnet/runtime/issues/52700");

		FileSystem.Initialize().WithFile(sourcePath)
			.Which(f => f.HasStringContent(sourceContents));
		IFileInfo sut = FileSystem.FileInfo.New(sourcePath);
		using (FileSystem.FileStream
			.New(sourcePath, FileMode.Open, fileAccess, FileShare.Write))
		{
			sut.CopyTo(destinationPath);
		}

		await That(FileSystem.File.Exists(destinationPath)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationPath)).IsEqualTo(sourceContents);
	}

	[Theory]
	[AutoData]
	public async Task
		CopyTo_SourceIsDirectory_ShouldThrowUnauthorizedAccessException_AndNotCopyFile(
			string sourceName,
			string destinationName)
	{
		FileSystem.Directory.CreateDirectory(sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.CopyTo(destinationName);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(sourceName)}'").And
			.WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}

	[Theory]
	[InlineAutoData(FileShare.None)]
	[InlineAutoData(FileShare.Write)]
	public async Task CopyTo_SourceLocked_ShouldThrowIOException(
		FileShare fileShare,
		string sourceName,
		string destinationName)
	{
		Skip.If(!Test.RunsOnWindows && fileShare == FileShare.Write,
			"see https://github.com/dotnet/runtime/issues/52700");

		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, fileShare);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.CopyTo(destinationName);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>().WithHResult(-2147024864);
			await That(FileSystem.File.Exists(destinationName)).IsFalse();
		}
		else
		{
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.Exists(destinationName)).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task CopyTo_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.CopyTo(destinationName);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessageContaining(Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'");
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}
}

using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class MoveToTests
{
	[Theory]
	[AutoData]
	public async Task MoveTo_DestinationExists_ShouldThrowIOException_AndNotMoveFile(
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
			sut.MoveTo(destinationName);
		}

		await That(Act).Throws<IOException>().WithHResult(Test.RunsOnWindows ? -2147024713 : 17);

		await That(sut.Exists).IsTrue();
		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[Theory]
	[AutoData]
	public async Task MoveTo_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		sut.MoveTo(destinationName, true);

		await That(sut.Exists).IsTrue();
		await That(sut.ToString()).IsEqualTo(destinationName);
		await That(sut.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(sourceContents);
	}
#endif

	[Theory]
	[AutoData]
	public async Task MoveTo_Itself_ShouldDoNothing(
		string sourceName,
		string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.MoveTo(sourceName);
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_Itself_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName)
	{
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.MoveTo(sourceName);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessageContaining(Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'");
		await That(FileSystem.File.Exists(sourceName)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_MissingDestinationDirectory_ShouldThrowDirectoryNotFoundException_AndNotMoveFile(
			string sourceName,
			string missingDirectory,
			string destinationName,
			string sourceContents)
	{
		string destinationPath =
			FileSystem.Path.Combine(missingDirectory, destinationName);
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.MoveTo(destinationPath);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);

		await That(sut.Exists).IsTrue();
		await That(FileSystem.File.Exists(sourceName)).IsTrue();
		await That(FileSystem.File.ReadAllText(sourceName)).IsEqualTo(sourceContents);
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_ReadOnly_ShouldMoveFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		sut.IsReadOnly = true;

		sut.MoveTo(destinationName);

		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
		await That(FileSystem.File.GetAttributes(destinationName)).HasFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.System)]
	public async Task MoveTo_ShouldAddArchiveAttribute_OnWindows(
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

		sut.MoveTo(destinationName);

		await That(FileSystem.File.GetAttributes(destinationName)).IsEqualTo(expectedAttributes);
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_ShouldKeepMetadata(
		string sourceName,
		string destinationName,
		string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText(sourceName, contents);
		DateTime sourceCreationTime = FileSystem.File.GetCreationTime(sourceName);
		DateTime sourceLastAccessTime = FileSystem.File.GetLastAccessTime(sourceName);
		DateTime sourceLastWriteTime = FileSystem.File.GetLastWriteTime(sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(1000);

		sut.MoveTo(destinationName);

		await That(FileSystem.File.GetCreationTime(destinationName)).IsEqualTo(sourceCreationTime);
		await That(FileSystem.File.GetLastAccessTime(destinationName)).IsEqualTo(sourceLastAccessTime);
		await That(FileSystem.File.GetLastWriteTime(destinationName)).IsEqualTo(sourceLastWriteTime);
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_ShouldMoveFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		sut.MoveTo(destinationName);

		await That(sut.FullName).IsEqualTo(FileSystem.Path.GetFullPath(destinationName));
		await That(sut.Exists).IsTrue();
		await That(FileSystem.File.Exists(sourceName)).IsFalse();
		await That(FileSystem.File.Exists(destinationName)).IsTrue();
		await That(FileSystem.File.ReadAllText(destinationName)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_ShouldNotAdjustTimes(string source, string destination)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

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

		await That(sut.CreationTime).IsEqualTo(expectedCreationTime);
		await That(sut.LastAccessTime).IsEqualTo(expectedLastAccessTime);
		await That(sut.LastWriteTime).IsEqualTo(expectedLastWriteTime);
		await That(creationTime).IsEqualTo(expectedCreationTime);
		await That(lastAccessTime).IsEqualTo(expectedLastAccessTime);
		await That(lastWriteTime).IsEqualTo(expectedLastWriteTime);
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
	public async Task MoveTo_SourceLocked_ShouldThrowIOException(
		FileAccess fileAccess,
		FileShare fileShare,
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(
			sourceName, FileMode.Open, fileAccess, fileShare);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.MoveTo(destinationName);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>().WithHResult(-2147024864);
			await That(FileSystem.File.Exists(sourceName)).IsTrue();
			await That(FileSystem.File.Exists(destinationName)).IsFalse();
		}
		else
		{
			await That(Act).DoesNotThrow();
			// https://github.com/dotnet/runtime/issues/52700
			await That(FileSystem.File.Exists(sourceName)).IsFalse();
			await That(FileSystem.File.Exists(destinationName)).IsTrue();
		}
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		void Act()
		{
			sut.MoveTo(destinationName);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining(Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'").And
			.WithHResult(-2147024894);
		await That(FileSystem.File.Exists(destinationName)).IsFalse();
	}
}

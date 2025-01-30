using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class MoveToTests
{
	[Theory]
	[AutoData]
	public void MoveTo_DestinationExists_ShouldThrowIOException_AndNotMoveFile(
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

		exception.Should().BeException<IOException>(
			hResult: Test.RunsOnWindows ? -2147024713 : 17);

		sut.Exists.Should().BeTrue();
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().BeEquivalentTo(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[Theory]
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
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(sourceContents);
	}
#endif

	[Theory]
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

	[Theory]
	[AutoData]
	public void MoveTo_Itself_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName)
	{
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(sourceName);
		});

		exception.Should().BeException<FileNotFoundException>(
			hResult: -2147024894,
			messageContains: Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'");
		FileSystem.File.Exists(sourceName).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void
		MoveTo_MissingDestinationDirectory_ShouldThrowDirectoryNotFoundException_AndNotMoveFile(
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

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);

		sut.Exists.Should().BeTrue();
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().BeEquivalentTo(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}

	[Theory]
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
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(contents);
		FileSystem.File.GetAttributes(destinationName).Should().HaveFlag(FileAttributes.ReadOnly);
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.System)]
	public void MoveTo_ShouldAddArchiveAttribute_OnWindows(
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

		FileSystem.File.GetAttributes(destinationName)
			.Should().Be(expectedAttributes);
	}

	[Theory]
	[AutoData]
	public void MoveTo_ShouldKeepMetadata(
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

		FileSystem.File.GetCreationTime(destinationName)
			.Should().Be(sourceCreationTime);
		FileSystem.File.GetLastAccessTime(destinationName)
			.Should().Be(sourceLastAccessTime);
		FileSystem.File.GetLastWriteTime(destinationName)
			.Should().Be(sourceLastWriteTime);
	}

	[Theory]
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
		FileSystem.File.ReadAllText(destinationName).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public void MoveTo_ShouldNotAdjustTimes(string source, string destination)
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

		sut.CreationTime.Should().Be(expectedCreationTime);
		sut.LastAccessTime.Should().Be(expectedLastAccessTime);
		sut.LastWriteTime.Should().Be(expectedLastWriteTime);
		creationTime.Should().Be(expectedCreationTime);
		lastAccessTime.Should().Be(expectedLastAccessTime);
		lastWriteTime.Should().Be(expectedLastWriteTime);
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
	public void MoveTo_SourceLocked_ShouldThrowIOException(
		FileAccess fileAccess,
		FileShare fileShare,
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(
			sourceName, FileMode.Open, fileAccess, fileShare);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(hResult: -2147024864);
			FileSystem.File.Exists(sourceName).Should().BeTrue();
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
		else
		{
			// https://github.com/dotnet/runtime/issues/52700
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
		}
	}

	[Theory]
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

		exception.Should().BeException<FileNotFoundException>(
			messageContains: Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'",
			hResult: -2147024894);
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}
}

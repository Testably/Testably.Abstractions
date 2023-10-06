using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CopyToTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void CopyTo_DestinationExists_ShouldThrowIOException_AndNotCopyFile(
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
			sut.CopyTo(destinationName);
		});

		exception.Should().BeException<IOException>(
			hResult: Test.RunsOnWindows ? -2147024816 : 17);
		sut.Should().Exist();
		FileSystem.Should().HaveFile(sourceName)
			.Which.HasContent(sourceContents);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void CopyTo_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		IFileInfo result = sut.CopyTo(destinationName, true);

		sut.Should().Exist();
		sut.FullName.Should().Be(FileSystem.Path.GetFullPath(sourceName));
		result.Should().Exist();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.Should().HaveFile(sourceName)
			.Which.HasContent(sourceContents);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(sourceContents);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void CopyTo_ReadOnly_ShouldCopyFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);
		sut.IsReadOnly = true;

		sut.CopyTo(destinationName);

		FileSystem.Should().HaveFile(sourceName);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(contents)
			.And.HasAttribute(FileAttributes.ReadOnly);
	}

	[SkippableTheory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.System)]
	public void CopyTo_ShouldAddArchiveAttribute_OnWindows(
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

		result.Attributes.Should().Be(expectedAttributes);
		FileSystem.File.GetAttributes(destinationName)
			.Should().Be(expectedAttributes);
	}

	[SkippableTheory]
	[AutoData]
	public void CopyTo_ShouldCopyFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(1000);

		IFileInfo result = sut.CopyTo(destinationName);

		sut.FullName.Should().Be(FileSystem.Path.GetFullPath(sourceName));
		sut.Should().Exist();
		result.Should().Exist();
		result.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
		FileSystem.Should().HaveFile(sourceName)
			.Which.HasContent(contents);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void CopyTo_ShouldKeepMetadata(
		string sourceName,
		string destinationName,
		string contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		FileSystem.File.WriteAllText(sourceName, contents);
		DateTime sourceCreationTime = FileSystem.File.GetCreationTime(sourceName);
		DateTime sourceLastWriteTime = FileSystem.File.GetLastWriteTime(sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(1000);

		DateTime updatedTime = TimeSystem.DateTime.Now;
		sut.CopyTo(destinationName);

		if (Test.RunsOnWindows)
		{
			FileSystem.File.GetCreationTime(destinationName)
				.Should().BeOnOrAfter(updatedTime.ApplySystemClockTolerance());
		}
		else
		{
			FileSystem.File.GetCreationTime(destinationName)
				.Should().BeOnOrAfter(sourceCreationTime.ApplySystemClockTolerance())
				.And.BeBefore(updatedTime);
		}

		if (Test.RunsOnMac)
		{
			FileSystem.File.GetLastAccessTime(destinationName)
				.Should().BeOnOrAfter(sourceCreationTime.ApplySystemClockTolerance())
				.And.BeBefore(updatedTime);
		}
		else
		{
			FileSystem.File.GetLastAccessTime(destinationName)
				.Should().BeOnOrAfter(updatedTime.ApplySystemClockTolerance());
		}

		FileSystem.File.GetLastWriteTime(destinationName)
			.Should().Be(sourceLastWriteTime);
	}

	[SkippableTheory]
	[AutoData]
	public void CopyTo_SourceIsDirectory_ShouldThrowUnauthorizedAccessException_AndNotCopyFile(
		string sourceName,
		string destinationName)
	{
		FileSystem.Directory.CreateDirectory(sourceName);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.CopyTo(destinationName);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			$"'{FileSystem.Path.GetFullPath(sourceName)}'",
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(sourceName);
		FileSystem.Should().NotHaveFile(destinationName);
	}

	[SkippableTheory]
	[AutoData]
	public void CopyTo_SourceLocked_ShouldThrowIOException(
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, FileShare.None);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.CopyTo(destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(hResult: -2147024864);
			FileSystem.Should().NotHaveFile(destinationName);
		}
		else
		{
			FileSystem.Should().HaveFile(sourceName);
			FileSystem.Should().NotHaveFile(destinationName);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CopyTo_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		Exception? exception = Record.Exception(() =>
		{
			sut.CopyTo(destinationName);
		});

		exception.Should().BeException<FileNotFoundException>(
			hResult: -2147024894,
			messageContains: Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.GetFullPath(sourceName)}'");
		FileSystem.Should().NotHaveFile(destinationName);
	}
}

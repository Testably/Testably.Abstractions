using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class CopyToTests
{
	[Theory]
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
	[Theory]
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

	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public void CopyTo_ShouldCopyFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText(sourceName, contents);
		IFileInfo sut = FileSystem.FileInfo.New(sourceName);

		TimeSystem.Thread.Sleep(EnsureTimeout);

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

	[Theory]
	[AutoData]
	public void CopyTo_ShouldKeepMetadata(
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

	[Theory]
	[InlineAutoData(FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.Read, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite)]
	[InlineAutoData(FileAccess.Write, FileShare.Read)]
	[InlineAutoData(FileAccess.Write, FileShare.ReadWrite)]
	public void CopyTo_SourceAccessedWithReadShare_ShouldNotThrow(
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

		FileSystem.File.Exists(destinationPath).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationPath).Should().Be(sourceContents);
	}

	[Theory]
	[InlineAutoData(FileAccess.Read)]
	[InlineAutoData(FileAccess.ReadWrite)]
	[InlineAutoData(FileAccess.Write)]
	public void CopyTo_SourceAccessedWithWriteShare_ShouldNotThrowOnLinuxOrMac(
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

		FileSystem.File.Exists(destinationPath).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationPath).Should().Be(sourceContents);
	}

	[Theory]
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

	[Theory]
	[InlineAutoData(FileShare.None)]
	[InlineAutoData(FileShare.Write)]
	public void CopyTo_SourceLocked_ShouldThrowIOException(
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

	[Theory]
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

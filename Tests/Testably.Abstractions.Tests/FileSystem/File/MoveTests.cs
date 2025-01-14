using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class MoveTests
{
	[SkippableTheory]
	[AutoData]
	public void Move_CaseOnlyChange_ShouldMoveFileWithContent(
		string name, string contents)
	{
		string sourceName = name.ToLowerInvariant();
		string destinationName = name.ToUpperInvariant();
		FileSystem.File.WriteAllText(sourceName, contents);

		FileSystem.File.Move(sourceName, destinationName);

		if (Test.RunsOnLinux)
		{
			// sourceName and destinationName are considered different only on Linux
			FileSystem.Should().NotHaveFile(sourceName);
		}

		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(contents);
		FileSystem.Directory.GetFiles(".").Should()
			.ContainSingle(d => d.Contains(destinationName, StringComparison.Ordinal));
	}

	[SkippableTheory]
	[AutoData]
	public void
		Move_DestinationDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException(
			string source)
	{
		FileSystem.Initialize()
			.WithFile(source);
		string destination = FileTestHelper.RootDrive(Test, "not-existing/path");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(source, destination);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_DestinationExists_ShouldThrowIOException_AndNotMoveFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, destinationName);
		});

		exception.Should().BeException<IOException>(
			hResult: Test.RunsOnWindows ? -2147024713 : 17);

		FileSystem.Should().HaveFile(sourceName)
			.Which.HasContent(sourceContents);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void Move_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		FileSystem.File.Move(sourceName, destinationName, true);

		FileSystem.Should().NotHaveFile(sourceName);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(sourceContents);
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void Move_ReadOnly_ShouldMoveFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Move(sourceName, destinationName);

		FileSystem.Should().NotHaveFile(sourceName);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(contents)
			.And.HasAttribute(FileAttributes.ReadOnly);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_ShouldMoveFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);

		FileSystem.File.Move(sourceName, destinationName);

		FileSystem.Should().NotHaveFile(sourceName);
		FileSystem.Should().HaveFile(destinationName)
			.Which.HasContent(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_ShouldNotAdjustTimes(string source, string destination)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(source, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.File.Move(source, destination);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(destination);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(destination);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(destination);

		creationTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
		lastAccessTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
		lastWriteTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_SourceAndDestinationIdentical_ShouldNotThrowException(string path)
	{
		FileSystem.Initialize()
			.WithFile(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(path, path);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Move_SourceDirectoryMissing_ShouldThrowFileNotFoundException(
		string missingDirectory,
		string sourceName,
		string destinationName)
	{
		string sourcePath = FileSystem.Path.Combine(missingDirectory, sourceName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourcePath, destinationName);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(sourcePath)}'",
			hResult: -2147024894);
		FileSystem.Should().NotHaveFile(destinationName);
	}

	[SkippableTheory]
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
	public void Move_SourceLocked_ShouldThrowIOException_OnWindows(
		FileAccess fileAccess,
		FileShare fileShare,
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(
			sourceName, FileMode.Open, fileAccess, fileShare);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(hResult: -2147024864);
			FileSystem.Should().HaveFile(sourceName);
			FileSystem.Should().NotHaveFile(destinationName);
		}
		else
		{
			// https://github.com/dotnet/runtime/issues/52700
			FileSystem.Should().NotHaveFile(sourceName);
			FileSystem.Should().HaveFile(destinationName);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void Move_SourceMissing_CopyToItself_ShouldThrowFileNotFoundException(
		string sourceName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, sourceName);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(sourceName)}'",
			hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, destinationName);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(sourceName)}'",
			hResult: -2147024894);
		FileSystem.Should().NotHaveFile(destinationName);
	}
}

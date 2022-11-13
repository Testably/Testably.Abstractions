using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class MoveTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
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
			FileSystem.File.Exists(sourceName).Should().BeFalse();
		}

		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void
		Move_DestinationDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException(
			string source)
	{
		FileSystem.Initialize()
			.WithFile(source);
		string destination = FileTestHelper.RootDrive("not-existing/path");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(source, destination);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
			.Which.HResult.Should().Be(-2147024893);
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

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(-2147024713);
		}
		else
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(17);
		}

		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
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

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
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

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.GetAttributes(destinationName)
			.Should().HaveFlag(FileAttributes.ReadOnly);
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_ShouldMoveFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);

		FileSystem.File.Move(sourceName, destinationName);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void Move_ShouldNotAdjustTimes(string source, string destination)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(source, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.File.Move(source, destination);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(destination);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(destination);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(destination);

		creationTime.Should()
			.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			.BeOnOrBefore(creationTimeEnd);
		lastAccessTime.Should()
			.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			.BeOnOrBefore(creationTimeEnd);
		lastWriteTime.Should()
			.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			.BeOnOrBefore(creationTimeEnd);
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

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should()
			.Contain($"'{FileSystem.Path.GetFullPath(sourcePath)}'");
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Move_SourceLocked_ShouldThrowIOException_OnWindows(
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, FileShare.Read);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>()
				.Which.HResult.Should().Be(-2147024864);
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
		else
		{
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
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

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should()
			.Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
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

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should()
			.Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}
}